var race = require('./../model/race');
var competition = require('./../model/competition');
var router = {};
var allType = ['tennis','badminton','pingpong'];
var crypto  = require('crypto');
var sendEmail = require('./../model/sendEmail');

router.session = competition.session;

//getAll
var getData = function(req) {
  var data = {
    title:req.query.title,
    date: req.query.time,
    matchType: req.query.format,
    matchLastTime:req.query.matchLastTime,
    place: req.query.place,
    placeContain: req.query.placeContain,
    sectionPerDay: req.query.sectionPerDay,
    seed:req.query.seed,
    matches:[]
  };
  switch(data.matchType) {
    case 'SingleElimination':
      data.matches.push({
        indexOfGroup:1,
        groups:[{
          round: 1,
          battles: race[data.matchType](req.query.ids)
        }]
      });
      break;
    case 'SingleCycle':
      data.matches.push({
        indexOfGroup:1,
        groups: race[data.matchType](req.query.ids)
      });
      break;
    case 'GroupLoop':
      data.matches=race[data.matchType](req.query);
      break;
  }
  if(data.matches) {
    data.matches.forEach(function(match) {
      match.groups.forEach(function(group) {
        group.battles.forEach(function(battle) {
          if(!battle.athleteB) {
            battle.winner = 1;
          }
        });
      });
    });
  }
  return data;
}

var convert = function(data) {
  var sheet = data['Sheet1'];
  var attrs = Object.keys(sheet[1]);
  var output = [];
  var name,value;
  sheet.forEach(function(obj,index) {
    if(index > 1) {
      var temp = {};
      attrs.forEach(function(attr) {
        if(typeof sheet[1][attr] === 'string') {
          name = sheet[1][attr];
        } else {
          name = sheet[1][attr]['#text'];
        }
        if(typeof obj[attr] === 'string' || typeof obj[attr] === 'undefined') {
          value = obj[attr];
        } else {
          value = obj[attr]['#text'];
        }
        temp[name] = value;
      })
      output.push(temp);
    }
  });
  output.sort(function(a,b) {
    return b['积分'] - a['积分'];
  });
  return output;
}
var getCallback = function(err,doc,res,group,round,people) {
  if(err) {
    console.log(err);
    res.json({
      state:false,
      errormessage:err
    });
  } else if(doc){
    var groups = [];
    doc.matches.forEach(function(match){
      groups.push({
        group:match.indexOfGroup,
        battles:match.groups[round-1].battles
      });
    });
    res.json({
      state:true,
      data: {
        groups: groups,
        round:round,
        athletes: people
      }
    });
  } else {
    res.json({
      state:false,
      errormessage: '未找到相关信息'
    })
  }
}
var callback = function(err,doc,res) {
  if(err) {
    console.log(err);
    res.json({
      state:false,
      errormessage: err
    })
  } else {
    res.json({
      state:true,
      data:doc
    });
  }
}

var sessionCheck = function(sessionID,resolve,reject,cb) {
  if(!sessionID) {
    resolve();
    return;
  }

  competition.sessionConfig.store.get(sessionID,function(err,doc) {
    if(err) {
      console.log(err);
      resolve();
    }
    else if(doc) {
      competition.userModel.findOne({username:doc.username}).
        exec(function(err,doc) {
          if(err) {
            console.log(err);
          } else if(!!doc) {
            cb();
            resolve();
            return;
          }
          resolve();
      });
    } else {
      resolve();
    }
  });
}

/***
 * router defined
 */
router.getAll = function(req,res) {
  var istrue = false;
  new Promise(function(resolve,reject){
    sessionCheck(req.sessionID,resolve,reject,function(){
      istrue = true;
    });
  }).then(function() {
    if(!istrue) {
      res.json({
        state:false,
        errormessage:'未登录或登陆已过期'
      });
      return;
    }
    var data = {};
    var count = 0;
    var errormessage = '';
    allType.forEach(function(type) {
      competition[type+'Model'].find({
        username: req.session.username
      }).exec(function(err,doc) {
        count++;
        if(err) {
          console.log(err);
        } else {
          data[type] = [];
          doc.forEach(function(d) {
            d.totalRound = d.matches[0].groups.length;
            d.matches = undefined;
            data[type].push(d);
          });
        }
        if(count === allType.length) {
          data.state = data !== {};
          res.json(data);
        }
      });
    })
  });
}

//get
router.get = function(req,res) {
  var istrue = false;
  new Promise(function(resolve,reject){
    sessionCheck(req.sessionID,resolve,reject,function(){
      istrue = true;
    });
  }).then(function() {
    if(!istrue) {
      res.json({
        state:false,
        errormessage:'未登录或登陆已过期'
      });
      return;
    }
    var group = parseInt(req.query.group,10);
    group = !group?1: group;
    var round = parseInt(req.query.round,10);
    round = !round?1: round;
    if(allType.includes(req.params.type)) {
      competition[req.params.type+'Model'].find({
        title: req.query.title,
        username: req.session.username
      }).populate({
        path:'matches.groups.battles.athleteA matches.groups.battles.athleteB'
      }).exec(function(err,doc) {
        competition.matchAthleteModel.findOne({
          title:req.query.title,
          username: req.session.username
        }).populate({
          path:"athletes"
        }).exec(function(err,d) {
          getCallback(err,doc[0],res,group,round,d.athletes);
        })
      });
    } else if(req.params.type === 'athlete') {
      competition.matchAthleteModel.findOne({
        title: req.query.title,
        username: req.session.username
      }).populate({
        path:'athletes'
      }).exec(function(err,doc) {
        callback(err,doc,res);
      });
    }
  });
}

//update
router.update = function(req,res) {
  var istrue = false;
  new Promise(function(resolve,reject){
    sessionCheck(req.sessionID,resolve,reject,function(){
      istrue = true;
    });
  }).then(function() {
    if(!istrue) {
      res.json({
        state:false,
        errormessage:'未登录或登陆已过期'
      });
      return;
    }
    var data = req.body;
    if(allType.includes(req.params.type)) {
      competition[req.params.type+'Model'].findOne({
        title:req.query.title,
        username: req.session.username
      }).exec(function(err,doc) {
        if(err) {
          res.json({
            state:false,
            errormessage:err
          });
          console.log(err);
        } else if(doc) {
          var isfind = false;
          var temp1,temp2;
          var index,index1,index2;
          for (index = 0; index < doc.matches.length; index++) {
            temp1 = doc.matches[index].groups;
            for (index1 = 0; index1 < temp1.length; index1++) {
              temp2 = temp1[index1].battles;
              for (index2 = 0; index2 < temp2.length; index2++) {
                if(temp2[index2]._id.toString() === req.query._Id) {
                  isfind = true;
                  break;
                }
              }
              if(isfind) break;
            }
            if(isfind) break;          
          }
          var update ={};
          var str = 'matches.'+index+'.groups.'+index1+'.battles.'+index2+'.winner';
          update[str] = req.query.win;
          competition[req.params.type+'Model'].findOneAndUpdate({
            title:req.query.title,
            username: req.session.username
          },update,{new:true}).exec(function(err,doc){
            callback(err,doc,res);
          });
        }
      });
    } else if(req.params.type === 'athlete') {
      var _id = new competition.ObjectId(data._id);
      delete data._id;
      var update = {};
      update.athlete = data;
      update._id = _id;
      competition.athleteModel.findOneAndUpdate({
        _id : update._id
      },update,{new:true}).exec(function(err,doc) {
        callback(err,doc,res);
      });
    } else {
      res.json({
        state:false,
        errormessage:'参数错误'
      })
    }
  });
}

//save

router.save = function(req,res) {
  var istrue = false;
  new Promise(function(resolve,reject){
    sessionCheck(req.sessionID,resolve,reject,function(){
      istrue = true;
    });
  }).then(function() {
    if(!istrue) {
      res.json({
        state:false,
        errormessage:'未登录或登陆已过期'
      });
      return;
    }
    req.query.athletes = convert(req.body['NewDataSet']);
    for (let i = 0; i < req.query.seed; i++) {
      req.query.athletes[i]['seedParticipantNum'] = req.query.seed - i;
    }
    req.query.ids = [];
    var people = [];
    new Promise(function(resolve,reject){
      var count = 0;
      req.query.athletes.forEach(function(athlete,index) {
        competition.athleteModel.findOneAndUpdate({
          'athlete.身份证':athlete['身份证']
        },{athlete:athlete},{upsert:true,new:true},function(err,doc) {
          count++;
          if(err) {
            throw err;
          } else if(doc) {
            req.query.ids.push(doc._id);
            people.push(doc);
          }
          if(count === req.query.athletes.length) {
            resolve();
          }
        });
      });
    }).then(function() {
      competition.matchAthleteModel.findOneAndUpdate({
        title: req.query.title,
        username: req.session.username
      },{
        title: req.query.title,
        username: req.session.username,
        matchType: req.query.format,
        athletes: req.query.ids
      },{upsert:true,new:true}).exec(function(err,doc) {
        if(err) {
          throw err;
        }
      });
    }).then(function() {
      var data = getData(req);
      if(data.matches === []) {
        res.json({
          state:false,
          errormessage:'Your post data produce no matches'
        });
        return;
      }
      if(!allType.includes(req.params.type)) {
        res.json({
          state:false,
          errormessage:'get/' + req.params.type +' is not validate'
        });
        return;
      }
      competition[req.params.type+'Model'].findOneAndUpdate({
        title:data.title,
        username: req.session.username
      },data,{upsert:true,new:true}).populate({
        path:'matches.groups.battles.athleteA matches.groups.battles.athleteB'
      }).exec(function(err,doc) {
        getCallback(err,doc,res,1,1,people);
      });
    });
  });
}

//delete
router.delete = function(req,res) {
  var istrue = false;
  new Promise(function(resolve,reject){
    sessionCheck(req.sessionID,resolve,reject,function(){
      istrue = true;
    });
  }).then(function() {
    if(!istrue) {
      res.json({
        state:false,
        errormessage:'未登录或登陆已过期'
      });
      return;
    }
    if(allType.includes(req.params.type)) {
      competition[req.params.type+'Model'].findOneAndRemove({
        title: req.query.title
      }).exec(function(err,doc) {
        if(err) {
          console.log(err);
        } else {
          competition.matchAthleteModel.remove({
            title:req.query.title,
            username: req.session.username
          }).exec(function(err) {
            callback(err,doc,res);
          })
        }
      });
    }
  });
}

//getNextRound
router.getNextRound = function(req,res) {
  var istrue = false;
  new Promise(function(resolve,reject){
    sessionCheck(req.sessionID,resolve,reject,function(){
      istrue = true;
    });
  }).then(function() {
    if(!istrue) {
      res.json({
        state:false,
        errormessage:'未登录或登陆已过期'
      });
      return;
    }
    if(req.body.length<2) {
      res.json({
        state:false,
        errormessage: 'data parse failed'
      });
      return;
    }

    var data=[];
    var group = parseInt(req.query.group,10);
    group = !group?1: group;
    var round = parseInt(req.query.round,10);
    round = !round?1: round;
    if(allType.includes(req.params.type)) {
      if(req.query.format === 'SingleElimination') {
        for (let i = 0; i < req.body.length; i+=2) {
          data.push({athleteA:new competition.ObjectId(req.body[i]),athleteB:new competition.ObjectId(req.body[i+1])});
        }
        var update={};
        update['matches.'+(group-1)+'.groups.' + round] = {
          round:round+1,
          battles:data
        }
        competition[req.params.type+'Model'].findOneAndUpdate({
          title:req.query.title,
          username: req.session.username,
          'matches.indexOfGroup': group,
          'matches.groups.round': round
        },update,{new:true}).populate({
          path:'matches.groups.battles.athleteA matches.groups.battles.athleteB'
        }).exec(function(err,doc) {
          getCallback(err,doc,res,group,round+1);
        });
      } else {
        competition[req.params.type+'Model'].findOne({
          title:req.query.title,
          username: req.session.username,
          'matches.indexOfGroup': group,
          'matches.groups.round': round
        }).populate({
          path:'matches.groups.battles.athleteA matches.groups.battles.athleteB'
        }).exec(function(err,doc) {
          getCallback(err,doc,res,group,round+1);
        });
      }
    }
  });
}

router.signin = function(req,res) {
  if(!req.body.password) return;

  var sha1 = crypto.createHash('sha1');
  sha1.update(req.body.password);
  req.body.password = sha1.digest('hex');
  competition.userModel.findOne({
    username: req.body.username,
    pwd: req.body.password
  }).exec(function(err,doc) {
    if(err) {
      console.log(err);
      res.json({
        state:false,
        errormessage: '服务器发生错误\n' + err
      })
    } else if(doc) {
      req.session.username = req.body.username;
      res.json({
        state:true
      });
    } else {
      res.json({
        state:false,
        errormessage: '用户名或密码不正确'
      });
    }
  });
}

router.signout = function(req,res) {
  var istrue = false;
  new Promise(function(resolve,reject){
    sessionCheck(req.sessionID,resolve,reject,function(){
      istrue = true;
    });
  }).then(function() {
    if(!istrue) {
      res.json({
        state:false,
        errormessage:'未登录'
      });
      return;
    }
    competition.sessionConfig.store.destroy(req.sessionID,function(err){
      res.clearCookie('connect.sid');
      res.json({
        state:!err,
        errormessage:err
      });
    });
  });
}

router.signup = function(req,res) {
  if(req.body) {
    competition.userModel.findOne({
      username: req.body.username
    }).exec(function(err,doc) {
      if(err) {
        console.log(err);
        res.json({
          state:false,
          message: '服务器发生错误\n' + err
        })
      } else if(doc) {
        res.json({
          state:false,
          message:'用户名已注册'
        });
      } else {
        req.session.username = req.body.username;
        var user = {
          username: req.body.username
        }
        var sha1 = crypto.createHash('sha1');
        sha1.update(req.body.password);
        user.pwd = sha1.digest('hex');
        var newUser = new competition.userModel(user);
        newUser.save(function(err) {
          if(err) {
            res.json({
              state:false,
              message: '服务器发生错误\n' + err
            });
          } else {
            req.session.username = req.body.username;
            if(req.body.email) {
              sendEmail(req.body.username,req.body.email);
            }
            res.json({
              state:true,
              message:'注册成功'
            });
          }
        });
      }
    });
  } else {
    res.json({
      state:false,
      message:'注册失败'
    })
  }
}

exports = module.exports = router;