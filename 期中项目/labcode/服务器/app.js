var express     = require('express');
var path        = require('path');
var favicon     = require('serve-favicon');
var logger      = require('morgan');
var cookieParser= require('cookie-parser');
var bodyParser  = require('body-parser');

var router      = require('./router/index');
var app = express();

logger.format('competition','[competition] :remote-addr - :remote-user [:date[clf]] ":method :url HTTP/:http-version" :status :res[content-length] ":referrer" ":user-agent"')

app.use(logger('competition'));
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(router.session);

app.get('/signout',router.signout);
app.post('/signin',router.signin);
app.post('/signup',router.signup);

app.get('/get/all', router.getAll);
app.get('/get/:type',router.get);
app.get('/delete/:type',router.delete);
app.get('/update/:type', router.update);
app.post('/update/:type', router.update);
app.post('/save/:type', router.save);
app.post('/getNextRound/:type',router.getNextRound);

app.get('*',function(req,res) {
  res.json({
    state:false,
    errormessage: '404 not found'
  });
});

module.exports = app;
