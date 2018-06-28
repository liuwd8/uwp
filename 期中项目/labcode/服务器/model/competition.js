'use strict'

var mongo = require("mongoose");
var promise = require('bluebird');
var session     = require('express-session');
var mongoStore  = require('connect-mongo')(session);
var crypto  = require('crypto');

var db = mongo.createConnection('mongodb://competition:sysu615@127.0.0.1:27017/competition');

db.on('connected', function () {
  console.log('数据库连接已建立');
});

db.on('error', function (err) {
  console.log('数据库连接异常:' + err);
});

db.on('disconnected', function () {
  console.log('数据库连接已断开');
});

var competition = {};
//运动员集合
var athleteSchma = new mongo.Schema({
  athlete : {type:Object,unique:true}
},{autoIndex:false,versionKey: false});
competition.athleteModel = db.model('athlete',athleteSchma);

//某场比赛的运动员集合
var matchAthleteSchma = new mongo.Schema({
  title: String,
  username: String,
  matchType: String,
  athletes : [{type:mongo.Schema.Types.ObjectId,ref:"athlete"}]
},{autoIndex:false,versionKey: false});
competition.matchAthleteModel = db.model('matchAthlete',matchAthleteSchma);

//对战骨架
var battleSchma = new mongo.Schema({
  athleteA:{type:mongo.Schema.Types.ObjectId,required: true,ref:"athlete"},
  athleteB:{type:mongo.Schema.Types.ObjectId,ref:"athlete",default:null},
  winner:{type:Number,default:0}
});
//比赛骨架
var matchSchma = new mongo.Schema({
  title: {type:String,required:true},
  username:{type:String,required:true},
  date: String,
  matchType: String,
  matchLastTime:String,
  place: String,
  placeContain:String,
  sectionPerDay: String,
  seed: {type:Number,default:0},
  matches: [{
    indexOfGroup: Number,
    groups: [{
      round: Number,
      battles:[battleSchma]
    }]
  }]
},{autoIndex:false,versionKey: false});

//三种比赛集合
competition.tennisModel = db.model('tennis',matchSchma);
competition.badmintonModel = db.model('badminton',matchSchma);
competition.pingpongModel = db.model('pingpong',matchSchma);
competition.ObjectId = mongo.Types.ObjectId;

//users
var userSchma = new mongo.Schema({
  username:{
    type:String,
    unique:true,
    required:true,
    unique:true,
    validate:/[a-zA-Z][a-zA-Z0-9]{5,15}/
  },
  pwd:{
    type:String,
    unique:true,
    required:true,
    validate:/[a-zA-Z](\S){5,15}/
  }
},{autoIndex:false,versionKey: false});
competition.userModel = db.model('user',userSchma);

competition.userModel.findOne({username:'admin1'}).exec(function(err,doc) {
  if(err) {
    console.log(err);
  } else if(!doc) {
    var admin = {
      username: 'admin1'
    }
    var sha1 = crypto.createHash('sha1');
    sha1.update('615coder');
    admin.pwd = sha1.digest('hex');
    var adminUser = new competition.userModel(admin);
    adminUser.save(function(err) {
      if(err) {
        console.log(err);
      }
    })
  }
});
competition.userModel.findOne({username:'admin2'}).exec(function(err,doc) {
  if(err) {
    console.log(err);
  } else if(!doc) {
    var admin = {
      username: 'admin2'
    }
    var sha1 = crypto.createHash('sha1');
    sha1.update('sys615');
    admin.pwd = sha1.digest('hex');
    var adminUser = new competition.userModel(admin);
    adminUser.save(function(err) {
      if(err) {
        console.log(err);
      }
    })
  }
});
//session
competition.sessionConfig = {
  secret: 'recommand 128 bytes random string',
  store: new mongoStore({
    mongooseConnection : db,
    ttl : 24 * 60 * 60 * 1000 //24h
  }),
  saveUninitialized: false,
  resave: false,
  cookie: {
    maxAge: 6 *  60 * 60 * 1000 //24h
  }
}
competition.session = session(competition.sessionConfig);

exports = module.exports = competition;