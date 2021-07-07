
const socket = io();

const coundown = (time, overtime)=>{
    var diff = time - Date.now();
    if(diff < 0){
        if(overtime == null) diff = 0;
        else return overtime;
    }
    var minutes = Math.floor((diff / 6e4));
    var seconds = Math.floor((diff / 1e3)) % 60;
    return `${('0'+minutes).slice(-2)}:${('0'+seconds).slice(-2)}`;
}

socket.on('message', args=>{
    const e = args[0];

    if(e == 'panel'){
        $('.office, .elevator, .game').hide();
        clearInterval(countdownInterval);
        panel[args[1]](args);
        $(`.${args[1]}`).show();

    }
});

var countdownInterval = null;
const panel = {
    office: args=>{
        $('.office span').text(`No one is in the elevator!\n${args[2]} sockets connected.`);
    },
    elevator: args=>{
        const game = args[2];
        
        $('.map-select .selected').removeClass('selected');
        $(`.map-select span:contains("${game.map.name}")`).addClass('selected');

        $('.elevator .players').text(`${args[3]}/12 players`);
        $('.elevator .outside').text(`(${args[4] - args[3]} in office)`);

        countdownInterval = setInterval(()=>$('.elevator .countdown').text(coundown(game.plannedStart, 'Starting Game...')), 1e3);
        $('.elevator .countdown').text(coundown(game.plannedStart, 'Starting Game...'));
    },
    game: args=>{
        const game = args[2];
        countdownInterval = setInterval(()=>$('.game .countdown').text(coundown(game.plannedEnd, 'Ending Game...')), 1e3);
        $('.game .countdown').text(coundown(game.plannedEnd, 'Ending Game...'));
    }
}

$('.map-select span').click(function(){
    if($(this).hasClass('selected')) return;
    $('.map-select .selected').removeClass('selected');
    $(this).addClass('selected');
    socket.emit('map-change', this.innerText);
});

$('.start').click(()=>socket.emit('start'));

$('.broadcast').keydown(e=>{
    if(e.key != 'Enter') return;
    socket.emit('broadcast', $('.broadcast').val());
    $('.broadcast').val('');
})

$('.end').click(()=>socket.emit('end'));