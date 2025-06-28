using System.Net;

namespace PCL.Neo.Core.Service.Accounts.Exceptions
{
    /// <summary>
    /// 外置登录认证过程中发生的异常
    /// </summary>
    public class YggdrasilAuthException : Exception
    {
        /// <summary>
        /// 发生异常的服务器URL
        /// </summary>
        public string ServerUrl { get; }
        
        /// <summary>
        /// 错误类型，由服务器返回
        /// </summary>
        public string ErrorType { get; }
        
        /// <summary>
        /// 原始错误内容
        /// </summary>
        public string ErrorContent { get; }
        
        /// <summary>
        /// HTTP状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;
        
        /// <summary>
        /// 是否需要重新登录
        /// </summary>
        public bool RequireRelogin { get; set; }
        
        /// <summary>
        /// 创建外置登录认证异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="serverUrl">服务器URL</param>
        /// <param name="errorType">错误类型</param>
        /// <param name="errorContent">错误内容</param>
        public YggdrasilAuthException(string message, string serverUrl, string errorType, string errorContent)
            : base(message)
        {
            ServerUrl = serverUrl;
            ErrorType = errorType;
            ErrorContent = errorContent;
        }
        
        /// <summary>
        /// 创建外置登录认证异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="serverUrl">服务器URL</param>
        /// <param name="errorType">错误类型</param>
        /// <param name="errorContent">错误内容</param>
        /// <param name="innerException">内部异常</param>
        public YggdrasilAuthException(string message, string serverUrl, string errorType, string errorContent, Exception innerException)
            : base(message, innerException)
        {
            ServerUrl = serverUrl;
            ErrorType = errorType;
            ErrorContent = errorContent;
        }
    }
} 