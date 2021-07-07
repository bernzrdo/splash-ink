const WebSocket = require('ws');
const {v4: uuid} = require('uuid');
const discord = require('discord.js');
const dashboard = require('./dashboard');

// ---------- CONFIGURATION ----------

const PORT = 4523;
const WAITING_MINUTES = 3;
const MATCH_TIME = 5;
const MAPS = [
    {
        name: 'Farmilicious',
        startPositions: {
            cow: [
                '19 1,5 0 0 -90 0',
                '19 1,5 1,5 0 -90 0',
                '19 1,5 -1,5 0 -90 0',
                '20,5 1,5 0 0 -90 0',
                '20,5 1,5 1,5 0 -90 0',
                '20,5 1,5 -1,5 0 -90 0'
            ],
            pug: [
                '-19 1,5 0 0 90 0',
                '-19 1,5 1,5 0 90 0',
                '-19 1,5 -1,5 0 90 0',
                '-20,5 1,5 0 0 90 0',
                '-20,5 1,5 1,5 0 90 0',
                '-20,5 1,5 -1,5 0 90 0'
            ]
        }
    },
    {
        name: 'Towerrific',
        startPositions: {
            cow: [
                '55 1,5 0 0 -90 0',
                '55 1,5 1,5 0 -90 0',
                '55 1,5 -1,5 0 -90 0',
                '56,5 1,5 0 0 -90 0',
                '56,5 1,5 1,5 0 -90 0',
                '56,5 1,5 -1,5 0 -90 0'
            ],
            pug: [
                '-55 1,5 0 0 90 0',
                '-55 1,5 1,5 0 90 0',
                '-55 1,5 -1,5 0 90 0',
                '-56,5 1,5 0 0 90 0',
                '-56,5 1,5 1,5 0 90 0',
                '-56,5 1,5 -1,5 0 90 0'
            ]
        }
    }
];

// ---------- VARIABLES ----------

const server = new WebSocket.Server({port: PORT},()=>console.log(`Server: Ready!`));

var game;
var interval;

var guild;
var discordCodes = {};

// ---------- FUNCTIONS ----------

function parse(message){
    var result = [''];
    var inQuote = false;
    message = message.trim();
    for(i in message){
        var isEscaped = message[i-1] == '\\';
        if(message[i] == ' ' && !inQuote && !isEscaped) result.push('');
        else if(/["']/.test(message[i]) && !isEscaped) inQuote = !inQuote;
        else result[result.length - 1] += message[i];
    }
    for(i in result) if(!isNaN(result[i]) && Number(result[i]).toString() === result[i]) result[i] = Number(result[i]);
    return result;
}

send = (s,m)=>s.send(parse(m).join('\n'));

function broadcast(message, exception = ()=>true){
    const parsedMessage = parse(message).join('\n');
    for(ws of [...server.clients].filter(exception)) ws.send(parsedMessage);
}

countPlayers = ()=>[...server.clients].filter(s=>s.data.inGame).length;

// ---------- SERVER ----------

server.on('connection',ws=>{

    // Prevent connection if Discord is not ready!
    if(guild == null){
        ws.terminate();
        return;
    }

    // Set default settings
    ws.data = {
        id: uuid(),
        team: 'c',
        inGame: false,
        isAlive: true
    }
    ws.on('pong',()=>ws.data.isAlive=true);
    ws.on('close',()=>leavePlayer(ws));

    bot.user.setActivity(`with ${server.clients.size} splasher${server.clients.size == 1 ? '' : 's'}!`);

    // Update player on game state
    if(game == null || !game.started){
        send(ws,'elevator open');
        send(ws,'hubText "Waiting for more players..."');
    }else send(ws,'hubText "Please wait until the current game ends"');

    updateDashboard();

    ws.on('message',message=>{

        console.log(`[${ws.data.discordName ?? '???'}] ${message}`);

        // Move player
        if(message.startsWith('m ')){
            broadcast(`m ${ws.data.id} ${message.substr(2)} ${ws.data.team} ${ws.data.discordName ?? ''}`, s=>s.data.inGame&&s.data.id!=ws.data.id);
            return;
        }

        // Shoot bullet
        if(message.startsWith('b ')){
            broadcast(`b ${ws.data.id} ${message.substr(2)}`, s=>s.data.inGame&&s.data.id!=ws.data.id);
            return;
        }

        // Damage
        if(message.startsWith('d ')){
            broadcast(`d ${message.replace(/^.+ /g, '')}`, s=>s.data.inGame && s.data.id == parse(message)[1])
            return;
        }

        // Hit animation
        if(message == 'h'){
            broadcast(`h ${ws.data.id}`, s=>s.data.inGame&&s.data.id!=ws.data.id);
            return;
        }

        // Bullet splashed
        if(message == 's' && game != null){
            game.scores[ws.data.team == 'c' ? 1 : 0]++;
            broadcast(`s ${game.scores.join(' ')}`, s=>s.data.inGame);
            return;
        }

        // PARSE
        message = parse(message);

        // HANDLE
        if(message[0] == 'join'){

            // Initialize game
            if(game == null){
                game = {
                    started: false,
                    plannedStart: Date.now() + WAITING_MINUTES*6e4,
                    map: MAPS[Math.floor(Math.random()*MAPS.length)],
                    scores: [0,0]
                }
                interval = setInterval(elevatorScreenUpdate, 1e3);
            }
            
            ws.data.inGame = true;

            dynamicVoiceChannelSystem(ws);
            updateDashboard();

            if(countPlayers() > 11) start();

            elevatorScreenUpdate();
        }

        else if(message[0] == 'leave') leavePlayer(ws);

        else if(message[0] == 'died') broadcast(`died ${ws.data.id}`, s=>s.data.inGame&&s.data.id!=ws.data.id);

        else if(message[0] == 'discord'){
            if(message[1] == 'i-am'){
                guild.members.fetch(message[2])
                    .then(member=>{
                        ws.data.discord = member;
                        ws.data.discordName = member.user.username;
                        send(ws, `discord info '${member.user.tag}' '${member.user.displayAvatarURL({
                            format: 'png',
                            dynamic: false,
                            size: 1024
                        })}'`);
                    })
                    .catch(()=>send(ws, 'discord not-found'));

            }else if(message[1] == 'code'){
                delete discordCodes[Object.keys(discordCodes).find(k=>discordCodes[k]==ws)];

                var code;
                do code = Math.floor(Math.random()*1e4).toString().padStart(4, 0);
                while(code in discordCodes);
                discordCodes[code] = ws;
                send(ws, `discord code ${code}`);

            }else if(message[1] == 'forget'){
                delete ws.data.discord;

            }
        }

    })

});

// Check when socket disconnects
const heartbeatCheck = setInterval(()=>{
    for(ws of server.clients){
        if(!ws.data.isAlive) return ws.termitate();
        ws.isAlive = false;
        ws.ping(()=>{});
    }
},3e3);

// Server close
server.on('close',()=>clearInterval(heartbeatCheck));

// Kick player out of game
function leavePlayer(ws){
    if(game != null){
        ws.data.inGame = false;
        if(countPlayers() == 0) game = null;
        else broadcast(`left ${ws.data.id}`, s=>s.data.inGame);
        dynamicVoiceChannelSystem(ws);
    }
    updateDashboard();
    if(guild != null) bot.user.setActivity(`with ${server.clients.size} splasher${server.clients.size == 1 ? '' : 's'}!`);
}

// Update elevator screen
function elevatorScreenUpdate(){
    if(game == null || game.started){
        broadcast('elevatorScreen ""');
        clearInterval(interval);
        return;
    }
    var diff = game.plannedStart - Date.now();
    if(diff < 0) start();
    else{
        var minutes = Math.floor((diff / 6e4));
        var seconds = Math.floor((diff / 1e3)) % 60;
        const countdown = `${('0'+minutes).slice(-2)}:${('0'+seconds).slice(-2)}`;
        broadcast(`elevatorScreen "Splash Ink.\\n\\n${countdown}\\n${countPlayers()}/12 players\\n\\nNEXT STATION:\\n${game.map.name.toUpperCase()}"`);
    }
}

// Update HUD
function hudUpdate(){
    if(game == null || !game.started){
        broadcast('hud ""');
        clearInterval(interval);
        return;
    }
    var diff = game.plannedEnd - Date.now();
    if(diff < 0) end();
    else{
        var minutes = Math.floor((diff / 6e4));
        var seconds = Math.floor((diff / 1e3)) % 60;
        const countdown = `${('0'+minutes).slice(-2)}:${('0'+seconds).slice(-2)}`;
        broadcast(`hud "${countdown}"`);
    }
}

// Start game
function start(){

    clearInterval(interval);
    game.plannedStart = Date.now() - 1;
    updateDashboard();

    // split players into teams
    var players = [...server.clients].filter(s=>s.data.inGame);
    for(p of players) p.data.team = 'c';
    const pugCount = Math[Math.random()<.5 ? 'floor' : 'ceil'](countPlayers() / 2);
    players = players.sort(()=>Math.random()-.5);
    for(i=0;i<pugCount;i++) players[i].data.team = 'p';
    for(p of players) send(p, `team ${p.data.team}`);

    // send all the info to start the game
    broadcast('elevatorScreen "Splash Ink.\\n\\nStarting game..."');
    broadcast('hubText "Starting game..."');
    broadcast('elevator close');

    setTimeout(()=>{

        var cowPosIndex = 0;
        var pugPosIndex = 0;
        for(ws of [...server.clients].filter(s=>s.data.inGame)){
            var pos;
            if(ws.data.team == 'c') pos = game.map.startPositions.cow[cowPosIndex++];
            else pos = game.map.startPositions.pug[pugPosIndex++];
            broadcast(`start ${game.map.name} ${pos}`, s=>s.data.id == ws.data.id);
            dynamicVoiceChannelSystem(ws);
        }

        setTimeout(()=>{
            // start and schedule end
            game.started = true;
            delete game.plannedStart;
            game.plannedEnd = Date.now() + MATCH_TIME*6e4;

            // update hub
            interval = setInterval(hudUpdate, 1e3)
            hudUpdate();

            // dashboard
            updateDashboard();
        }, 3e3);
    },3e3);
}

// End game
function end(){
    clearInterval(interval);
    broadcast(`end ${game.scores.join(' ')}`, s=>s.data.inGame);

    for(ws of [...server.clients].filter(s=>s.data.inGame)) leavePlayer(ws);
    game = null;

    updateDashboard();

    broadcast('elevator open');
    broadcast('hubText "Waiting for more players..."');
}

// ---------- DISCORD ----------

const bot = new discord.Client();
bot.on('ready',()=>{
    console.log('Discord: Ready!');
    bot.user.setActivity(`with ${server.clients.size} splasher${server.clients.size == 1 ? '' : 's'}!`);
    bot.guilds.fetch('810237725776019506').then(g=>guild=g);
});

bot.on('message', message=>{

    if(message.channel.id == '810270714338476122' && !message.author.bot){
        var descrition = message.content.split('\n');
        var title = descrition.shift();
        descrition = descrition.join('\n');

        message.channel.send(new discord.MessageEmbed()
            .setColor('#9c59b6')
            .setTitle(title)
            .setDescription(descrition)
            .setTimestamp()
        );

        message.delete();
        return;
    }

    if(message.author.bot || !message.content.startsWith('.')) return;

    const args = parse(message.content.substr(1));

    if(args[0] == 'link'){
        if(args[1] in discordCodes){
            send(discordCodes[args[1]], `discord link '${message.member.id}'`);
            delete discordCodes[args[1]];
            message.channel.send('Your Discord was successfully linked!');
        }else message.channel.send('I couldn\'t find that code!')
    }

});

bot.on('voiceStateUpdate', (oldState, newState)=>{

    var ws = [...server.clients].find(s=>s.data.discord == newState.member);
    if(ws == null) return;
    dynamicVoiceChannelSystem(ws);

});

function dynamicVoiceChannelSystem(ws){
    if(ws.data.discord == null || ws.data.discord.voice.channelID == null) return;

    var vc;

    if(ws.data.inGame && game != null){
        if(game.started) vc = ws.data.team == 'c' ? '810241679218442242' : '810241704145846272' // Game
        else vc = '810241650076811326'; // Elevator
    }else vc = '810241616723050566'; // Office

    if(ws.data.discord.voice.channelID == vc) return;
    ws.data.discord.voice.setChannel(vc);
}

bot.login(discordBotToken);

// ---------- DASHBOARD ----------

function updateDashboard(){
    if(game == null) dashboard.emit('panel', 'office', server.clients.size);
    else if(!game.started) dashboard.emit('panel', 'elevator', game, countPlayers(), server.clients.size);
    else dashboard.emit('panel', 'game', game);
}
dashboard.events.on('update', updateDashboard);

dashboard.events.on('start', start);
dashboard.events.on('map-change', map=>{
    game.map = MAPS.find(m=>m.name == map);
    elevatorScreenUpdate();
});
dashboard.events.on('end', end);

dashboard.events.on('broadcast', msg=>broadcast(msg))
