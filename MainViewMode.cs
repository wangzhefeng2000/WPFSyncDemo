using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using static System.Net.WebRequestMethods;

namespace WPFSyncDemo
{
    public class MainViewModel : INotifyPropertyChanged
{
        string url1 = "http://www.example.com";
        string url2 = "http://123.139.43.74:27888/V2.4.3/云智八局客户端安装程序.exe";

        public MainViewModel()
        {
            //UrlByteCount = new NotifyTaskCompletion<int>(MyService.DownloadAndCountBytesAsync(url2));
            CreateCmd();
        }
        public void CreateCmd()
        {
            // 方法1，可行
            Status1 = new NotifyTaskCompletion<string>(Task.FromResult("同步cmd，运行计算器"));
            Status2 = new NotifyTaskCompletion<string>(Task.FromResult("同步cmd，打开文本"));
            Cmd1 = new Command(() =>
            {
                Status1 = new NotifyTaskCompletion<string>(MyTask.RunCalc(null));
                Status2 = new NotifyTaskCompletion<string>(MyTask.RunNotepad(null));
            });

            // 方法2，这种方法更好。推荐
            Cmd2 = AsyncCommand.Create( async () =>            
            {
                String1 = "异步Cmd，启动计算器";
                String2 = "异步Cmd，打开文本";

                IsLoading = true;
                var t1 = MyTask.RunCalc( s =>  String1 = s  );
                var t2 = MyTask.RunNotepad(s => String2 = s);

                await Task.WhenAll(t1, t2);
                IsLoading = false;
            }
            );

            String1 = "计算器";
            String2 = "文本";


            //======================
            Cmd3 = new Command(() =>
            {
                UrlByteCount = new NotifyTaskCompletion<int>(
                    MyService.DownloadAndCountBytesAsync(url1)
                    //,(s, e) => RaisePropertyChanged("UrlByteCount")
                    );
            });
                   
        }

        private NotifyTaskCompletion<int> _urlByteCount;
        public NotifyTaskCompletion<int> UrlByteCount 
        { 
            get => _urlByteCount; 
            set
            {
                _urlByteCount = value;
                RaisePropertyChanged();
            }
        }

        private NotifyTaskCompletion<string> _status1;
        public NotifyTaskCompletion<string> Status1
        {
            get => _status1;
            set
            {
                _status1 = value;
                RaisePropertyChanged();
            }
        }

        private NotifyTaskCompletion<string> _status2;
        public NotifyTaskCompletion<string> Status2
        {
            get => _status2;
            set
            {
                _status2 = value;
                RaisePropertyChanged();
            }
        }

        private string _string1;
        public string String1
        {
            get => _string1;
            set
            {
                _string1 = value;
                RaisePropertyChanged();
            }
        }

        private string _string2;
        public string String2
        {
            get => _string2;
            set
            {
                _string2 = value;
                RaisePropertyChanged();
            }
        }

        private bool _isLoading ;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                RaisePropertyChanged();
            }
        }

        public ICommand Cmd1 { get; set; }
        public ICommand Cmd2 { get; set; }

        public ICommand Cmd3 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public static class MyService
    {
        // http://blogs.msdn.com/b/lucian/archive/2012/12/08/await-httpclient-getstringasync-and-cancellation.aspx
        public static async Task<int> DownloadAndCountBytesAsync(string url, CancellationToken token = new CancellationToken())
        {
            await Task.Delay(TimeSpan.FromSeconds(3), token).ConfigureAwait(false);
            var client = new HttpClient();
            using (var response = await client.GetAsync(url, token).ConfigureAwait(false))
            {
                var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return data.Length;
            }
        }
    }

    public static class MyTask
    {
        public static async Task<string> RunNotepad(
            Action<string> promptMessage,
            CancellationToken token = new CancellationToken())
        {
            await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
            promptMessage?.Invoke("正在处理文本");
            var p = Process.GetProcessesByName("Notepad").FirstOrDefault();
            if (p == null || p.HasExited)
            {
                Process.Start("Notepad");
            }
            return await GetStatus("Notepad", promptMessage).ConfigureAwait(false);
        }

        public static async Task<string> RunCalc(  
            Action<string> promptMessage,
            CancellationToken token = new CancellationToken())
        {
            await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
            promptMessage?.Invoke("正在计算");
            var p = Process.GetProcessesByName("Calculator").FirstOrDefault();
            if (p == null || p.HasExited)
            {
                  Process.Start("calc");
            }
            return await GetStatus("Calculator", promptMessage).ConfigureAwait(false);
        }

        public  static async Task<string> GetStatus(string procName, Action<string> promptMessage)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                var p = Process.GetProcessesByName(procName).FirstOrDefault();
                if (p == null || p.HasExited)
                {
                    break;                    
                }
            }
            promptMessage?.Invoke($"{procName}运行结束");
            return $"{procName}运行结束";
        }
    }
}
