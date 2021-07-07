const express = require('express');
const app = express();
const http = require('http').Server(app);
const io = require('socket.io')(http);
const events = require('events');
const eventEmitter = new events.EventEmitter();

app.use(express.static('dashboard'));

io.on('connection', socket=>{

    eventEmitter.emit('update');

    socket.on('map-change', map=>eventEmitter.emit('map-change', map));
    socket.on('start', ()=>eventEmitter.emit('start'));
    socket.on('broadcast', msg=>eventEmitter.emit('broadcast', msg));
    socket.on('end', ()=>eventEmitter.emit('end'));

});

http.listen(8080, ()=>console.log('Dashboard: Ready!'));

module.exports.emit = (...args)=>io.emit('message', args);
module.exports.events = eventEmitter;