using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MemSourceConnector.Api
{
    public class MemsourceApi
    {
        #region fields

        private readonly AuthInfo _authInfo;
        private readonly string _url;

        #endregion //fields

        #region constructor

        public MemsourceApi(string url, string login, string password)
        {
            if (!url.EndsWith("/"))
                url += "/";

            _url = url;

            var authBody = new AuthBody() { UserName = login, Password = password };
            _authInfo = AuthUserRequest(authBody);
        }

        #endregion //constructor

        /// <summary>
        /// Auth user
        /// </summary>
        /// <param name="authBody">Auth parameters</param>
        /// <returns>Auth description</returns>
        private AuthInfo AuthUserRequest(AuthBody authBody)
        {
            var auth = JsonConvert.SerializeObject(authBody);
            var body = Encoding.UTF8.GetBytes(auth);

            var request = (HttpWebRequest)WebRequest.Create($"{_url}v1/auth/login");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";
            request.ContentLength = body.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(body, 0, body.Length);
                stream.Close();
            }

            AuthInfo res = null;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var receiveStream = response.GetResponseStream())
                {
                    using (var readStream = new StreamReader(receiveStream))
                    {
                        var rqBody = readStream.ReadToEnd();
                        res = JsonConvert.DeserializeObject<AuthInfo>(rqBody);
                    }
                }

                response.Close();
            }

            return res;
        }

        /// <summary>
        /// Get TransMemories page
        /// </summary>
        /// <returns></returns>
        public TransMemories GetTransMemories(int pageNumber)
        {
            var request = (HttpWebRequest)WebRequest.Create($"{_url}v1/transMemories?pageNumber={pageNumber}");
            request.Method = WebRequestMethods.Http.Get;
            request.Headers.Add("Authorization", $"ApiToken {_authInfo.token}");

            TransMemories res = null;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var receiveStream = response.GetResponseStream())
                {
                    using (var readStream = new StreamReader(receiveStream))
                    {
                        var rqBody = readStream.ReadToEnd();
                        res = JsonConvert.DeserializeObject<TransMemories>(rqBody);
                    }
                }

                response.Close();
            }

            return res;
        }

        /// <summary>
        /// Get TransMemories DBs
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TM_Content> GetTransMemories()
        {
            var res = new List<TM_Content>();

            var first = GetTransMemories(0);

            if (first != null)
            {
                res.AddRange(first.content);

                if (first.totalPages != 1)
                {
                    for (var i = 1; i < first.totalPages; i++)
                    {
                        var page = GetTransMemories(i);
                        if (page != null)
                            res.AddRange(page.content);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Download TransMemories Dbs
        /// </summary>
        /// <param name="ids">Db to download</param>
        /// <param name="expType">Export type</param>
        /// <param name="folder">Output folder</param>
        /// <param name="states">States handler</param>
        public Task<string[]> ExportTransMemories(IEnumerable<TM_Content> ids, ExportType expType, string folder, bool zip, Action<string> states)
        {
            return Task.Run(() =>
            {
                var files = new List<string>();

                Parallel.ForEach(ids, id =>
                {
                    var sb = new StringBuilder();

                    var authBody = JsonConvert.SerializeObject(new ExportTransMemoriesBody() { callbackUrl = "", exportTargetLangs = id.targetLangs });
                    var body = Encoding.UTF8.GetBytes(authBody);

                    var filename = GetFilename($"{id.name}_{id.sourceLang}_{string.Format("_", id.targetLangs)}.{expType}");

                    try
                    {
                        sb.AppendLine($"{DateTime.Now.ToLongTimeString()}: запрос на экспорт БД {filename}");

                        var request = (HttpWebRequest)WebRequest.Create($"{_url}v2/transMemories/{id.id }/export");
                        request.Headers.Add("Authorization", $"ApiToken {_authInfo.token}");
                        request.Method = WebRequestMethods.Http.Post;
                        request.ContentType = "application/json";
                        request.ContentLength = body.Length;

                        using (Stream stream = request.GetRequestStream())
                        {
                            stream.Write(body, 0, body.Length);
                            stream.Close();
                        }

                        ExportTransMemoriesResponse res = null;

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            using (var receiveStream = response.GetResponseStream())
                            {
                                using (var readStream = new StreamReader(receiveStream))
                                {
                                    var rqBody = readStream.ReadToEnd();
                                    res = JsonConvert.DeserializeObject<ExportTransMemoriesResponse>(rqBody);
                                }
                            }

                            response.Close();
                        }

                        sb.AppendLine($"{DateTime.Now.ToLongTimeString()}: скачивание БД {filename}");

                        var file = DownloadExportTransMemoriesResult(res.asyncRequest.id, expType, e => sb.AppendLine(e));

                        if (file?.Any() != true)
                        {
                            sb.AppendLine($"{DateTime.Now.ToLongTimeString()}: ошибка загрузки {filename}. Нет данных");
                            return;
                        }

                        sb.AppendLine($"{DateTime.Now.ToLongTimeString()}: запись БД {filename} в файл");

                        //if (res.asyncRequest.project != null && !string.IsNullOrEmpty(res.asyncRequest.project.name))
                        //    folder = Path.Combine(folder, res.asyncRequest.project.name);

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        var filenameTmp = Path.Combine(folder, filename);

                        if (File.Exists(filenameTmp))
                            filenameTmp += DateTime.Now.Ticks;

                        File.WriteAllBytes(filenameTmp, file);

                        files.Add(filenameTmp);

                        sb.AppendLine($"{DateTime.Now.ToLongTimeString()}: загрузка {filename} завершена");

                        states.Invoke(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        states.Invoke($"{DateTime.Now.ToLongTimeString()}: ошибка загрузки {filename}. {ex.Message}");
                    }
                });

                return files.ToArray();
            });
        }

        private string GetFilename(string source)
        {
            try
            {
                source = source.Replace("\\", " ").Replace("/", " ");

                var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + new string(new[] { '\\', '/' });
                var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

                return r.Replace(source, "");
            }
            catch
            { }

            return source;
        }

        private AsyncRequest GetAsyncRequest(string asyncRequestId)
        {
            var request = (HttpWebRequest)WebRequest.Create($"{_url}v1/async/{asyncRequestId}");
            request.Method = WebRequestMethods.Http.Get;
            request.Headers.Add("Authorization", $"ApiToken {_authInfo.token}");

            AsyncRequest res = null;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var receiveStream = response.GetResponseStream())
                {
                    using (var readStream = new StreamReader(receiveStream))
                    {
                        var rqBody = readStream.ReadToEnd();
                        res = JsonConvert.DeserializeObject<AsyncRequest>(rqBody);
                    }
                }

                response.Close();
            }

            return res;
        }

        private byte[] DownloadExportTransMemoriesResult(string asyncRequestId, ExportType expType, Action<string> states)
        {
            byte[] res = null;
            var count = 0;

            while (true)
            {
                string msg = string.Empty;

                try
                {
                    var rq = GetAsyncRequest(asyncRequestId);

                    if (rq.asyncResponse != null)
                    {
                        msg = $"erroCode={rq.asyncResponse.errorCode ?? ""} errorDescription={rq.asyncResponse.errorDesc ?? ""}";

                        states.Invoke($"{DateTime.Now.ToLongTimeString()}: получения асинхронного запроса: {msg}");

                        var request = (HttpWebRequest)WebRequest.Create($"{_url}v1/transMemories/downloadExport/{asyncRequestId}?format={expType}");
                        request.Method = WebRequestMethods.Http.Get;
                        request.Headers.Add("Authorization", $"ApiToken {_authInfo.token}");

                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            using (var receiveStream = response.GetResponseStream())
                            {
                                var read = new byte[256];
                                int c = receiveStream.Read(read, 0, 256);
                                using (var ms = new MemoryStream())
                                {
                                    while (c > 0)
                                    {
                                        ms.Write(read, 0, c);
                                        c = receiveStream.Read(read, 0, 256);
                                    }

                                    res = ms.ToArray();
                                }
                            }

                            response.Close();

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    states.Invoke($"{DateTime.Now.ToLongTimeString()}: ошибка получения асинхронного запроса: {ex.Message}");
                    msg = ex.Message;
                }

                count++;

                if (count > 175)
                    throw new Exception(msg);

                Thread.Sleep(2000);
            }

            return res;
        }
    }
}
