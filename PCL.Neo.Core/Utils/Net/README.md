# NetService 和 StaticNet 使用文档

## NetService

### 概述
NetService 是一个基于依赖注入模式的 HTTP 客户端服务实现，提供了丰富的 HTTP 请求功能，包括发送普通请求、发送 JSON 数据以及处理响应。

### 使用说明

#### 构造 NetService 实例
NetService 需要通过依赖注入来使用。

```csharp
// 手动实例化
INetService netService = new NetService("MyHttpClient", "https://api.example.com", "my-token");
```

#### 发送 HTTP 请求

1. 发送无内容的请求：

```csharp
var response = await netService.SendAsync(HttpMethod.Get, "https://api.example.com/resource");
```

2. 发送带内容的请求：

```csharp
var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/resource")
{
    Content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json")
};
var response = await netService.SendAsync(request);
```

3. 发送 JSON 数据并获取反序列化结果：

```csharp
var result = await netService.SendJsonAsync<MyResponseType, MyRequestType>(HttpMethod.Post, "https://api.example.com/resource", new MyRequestType { Key = "value" });
```

### 注意事项
1. **依赖注入**：推荐使用依赖注入来管理 NetService 实例。
2. **令牌管理**：确保传递正确的令牌以进行身份验证。
3. **异常处理**：捕获可能的网络异常，例如 `HttpRequestException`。
4. **线程安全**：NetService 的方法是线程安全的，可以在多线程环境中使用。

---

## StaticNet

### 概述
StaticNet 是一个静态类，提供了简单易用的 HTTP 客户端服务功能，适合快速实现 HTTP 请求。

### 使用说明

#### 发送 HTTP 请求

1. 发送无内容的请求：

```csharp
var response = await StaticNet.SendAsync(HttpMethod.Get, new Uri("https://api.example.com/resource"));
```

2. 发送带内容的请求：

```csharp
var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/resource")
{
    Content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json")
};
var response = await StaticNet.SendAsync(request);
```

3. 发送 JSON 数据并获取反序列化结果：

```csharp
var result = await StaticNet.SendJsonAsync<MyResponseType, MyRequestType>(HttpMethod.Post, new Uri("https://api.example.com/resource"), new MyRequestType { Key = "value" });
```

### 注意事项
1. **静态类**：StaticNet 是静态类，无需通过依赖注入管理。
2. **令牌管理**：确保传递正确的令牌以进行身份验证。
3. **异常处理**：捕获可能的网络异常，例如 `HttpRequestException`。
4. **线程安全**：StaticNet 的方法是线程安全的，可以在多线程环境中使用。
