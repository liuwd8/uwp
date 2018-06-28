var nodemailer = require("nodemailer");
var smtpTransport = require('nodemailer-smtp-transport');

var email = function (username,email) {
	// 开启一个 SMTP 连接池
	var transport = nodemailer.createTransport(smtpTransport({
	  host: "smtp.163.com", // 主机
	  secure: true, // 使用 SSL
	  port: 465, // SMTP 端口
	  auth: {
	    user: "aaa998889@163.com", // 账号
	    pass: "sysu615" // 密码
	  }
	}));
	
	// 设置邮件内容
	var mailOptions = {
	  from: "Competition开发小组<aaa998889@163.com>", // 发件地址
	  to: email, // 收件列表
	  subject: "注册成功", // 标题
	  html: "尊敬的用户<b>" + username + "</b>:\n您好！\n欢迎使用competition系统 " // html 内容
	}
	
	// 发送邮件
	transport.sendMail(mailOptions, function(error, response) {
	  if (error) {
	    console.error(error);
	  } else {
	    console.log(response);
	  }
	  transport.close(); // 如果没用，关闭连接池
	});
}

exports = module.exports = email;