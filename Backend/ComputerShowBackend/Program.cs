using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// 添加 Swagger/OpenAPI 支持
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SmtpClientService>();

var app = builder.Build();

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 定义发送邮件的 API 端点
app.MapPost("/sendemail", async (SmtpClientService smtpService, [FromBody] EmailRequest request) =>
{
    try
    {
        await smtpService.SendEmailAsync(request.Content);
        Console.WriteLine("邮件发送成功!");
        return Results.Ok("邮件发送成功!"); 
    }
    catch (Exception ex)
    {
        Console.WriteLine("邮件发送失败:"+ex.Message);
        return Results.BadRequest($"邮件发送失败: {ex.Message}");
    }
})
.WithName("SendEmail")
.WithOpenApi();

app.Run();

// 数据模型：接收邮件内容的请求体
public record EmailRequest(string Content);

// SMTP 客户端服务类
public class SmtpClientService
{
    private readonly IConfiguration _configuration;

    public SmtpClientService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string content)
    {
        // QQ 邮箱 SMTP 配置
        var smtpServer = "smtp.qq.com";
        var smtpPort = 587; // QQ 邮箱使用 587 端口 (TLS)
        var fromEmail = _configuration["Smtp:Email"] ?? "your-qq-email@qq.com"; // 配置发件人邮箱
        var password = _configuration["Smtp:Password"] ?? "your-smtp-auth-code"; // QQ 邮箱的授权码

        // 收件人邮箱
        var toEmail = _configuration["ToEmail"] ??"2216528769@qq.com";

        // 创建邮件消息
        using var mailMessage = new MailMessage(fromEmail, toEmail)
        {
            Subject = "电脑维修通知",
            Body = content,
            IsBodyHtml = true
        };

        // 配置 SMTP 客户端
        using var smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true 
        };

        // 发送邮件
        await smtpClient.SendMailAsync(mailMessage);
    }
}