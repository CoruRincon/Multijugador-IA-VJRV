var ctx;
var sx,sy,sw,sh,dx,dy,dw,dh,p=0,paso=8;
var frameIndex=0,totalFrames=8;
sx=0 //Source  x. Frame index times frame width
sy=0 //Source y
sw=71; //Frame width 
sh=112; //Frame height
dx=0; //Destination x
dy=480; //Destination y
dw=71; //Destination width
dh=112; // Destination height

var ssx=0;			
//Creo una imagen conun objeto Image de Javascript			
//Posici�n y donde empieza el siguiente frame del sprite.
/*var v= [];
v[0] = 0;
v[1] = 100;
v[2] = 200;
v[3] = 300;
v[4] = 400;*/
var playerSprite = new Image();
var bg = new Image();
var v=new Array();
var moneda=new Array();
var explosion=new blast(100,100);
var refreshIntervalId;
var r;
var colision=false;

//---------------------------------------------------------------------------
function cargaContextoCanvas(idCanvas){
   var elemento = document.getElementById(idCanvas);
   if(elemento && elemento.getContext){
	  var contexto = elemento.getContext('2d');
	  if(contexto){
		 return contexto;
	  }
   }
   return FALSE;
}
window.addEventListener("keydown", checkKeyPressed, false);	

//---------------------------------------------------------------------------	
function azar(h){return parseInt(Math.random()*h);}
//---------------------------------------------------------------------------
function blast(xPos,yPos)//posicion x,y y numero de fotograma
{
	//Atributos
	this.xPos=xPos;
	this.yPos=yPos;
	this.imagen=new Image();				
	this.imagen.src = "Assets/Imgs/Sprites/blast.png";			
	this.frameIndex=0;
	this.totalFrames=14;			
	this.pintar=function(a)
	{	this.xPos=a;
		this.yPos=377;
		ssx=170*this.frameIndex;
		ctx.drawImage(bg,this.xPos,this.yPos,170,233,this.xPos,this.yPos,170,233);
		ctx.drawImage(this.imagen,ssx,0,170,233,this.xPos,this.yPos,170,233);
		this.frameIndex=(this.frameIndex+1) % this.totalFrames;					
	}
}

function coin(xPos,yPos)//posicion x,y y numero de fotograma
{
	//Atributos
	this.xPos=xPos;
	this.yPos=yPos;
	this.imagen=new Image();				
	this.imagen.src = "Assets/Imgs/Sprites/coin.png";			
	this.frameIndex=azar(19);
	this.totalFrames=19;
	this.size=50;
	this.pintar=function()
	{
		ssx=100*this.frameIndex;
		ctx.drawImage(bg,this.xPos,this.yPos-5,this.size,this.size,this.xPos,this.yPos-5,this.size,this.size);
		ctx.drawImage(this.imagen,ssx,0,100,100,this.xPos,this.yPos,this.size,this.size);
		this.frameIndex=(this.frameIndex+1) % this.totalFrames;
		this.yPos+=5
		if (this.yPos>700){this.yPos=-1400;this.xPos=azar(700);}
	}
	
}
function e(){colision=false;}

function bomba(xPos,yPos)//posicion x,y y numero de fotograma
{
	//Atributos
	this.xPos=xPos;
	this.yPos=yPos;
	this.imagen=new Image();				
	this.imagen.src = "Assets/Imgs/Sprites/bomba.png";			
	this.frameIndex=azar(19);
	this.totalFrames=19;
	this.size=azar(5)*10+22;
	
	this.pintar=function(alfa)
	{
		ssx=72*this.frameIndex;
		ctx.drawImage(bg,this.xPos,this.yPos-5,this.size,this.size,this.xPos,this.yPos-5,this.size,this.size);
		ctx.drawImage(this.imagen,ssx,0,72,72,this.xPos,this.yPos,this.size,this.size);
		this.frameIndex=(this.frameIndex+1) % this.totalFrames;
		this.yPos+=5
		if (this.yPos>500-this.size)
		{	
			if (((this.xPos>dx+p) && (this.xPos<dx+p+72)) || 
			    ((this.xPos+this.size>dx+p) && (this.xPos+this.size<dx+p+72))) //colision
			{
				//clearInterval(refreshIntervalId);
				colision=true;
				
				
				r=dx+p-30;
				ctx.drawImage(bg,0,0,800,600,0,0,800,600);// el fondo es de 800*600
				d();
				enviarBlast();
				setTimeout(d,100);							
				setTimeout(d,200);
				
				setTimeout(d,300);
				setTimeout(d,400);							
				setTimeout(d,500);	
				setTimeout(d,600);
				setTimeout(d,700);							
				setTimeout(d,800);	
				setTimeout(d,900);
				setTimeout(d,1000);							
				setTimeout(d,1100);	
				setTimeout(d,1200);
				setTimeout(d,1300);
				setTimeout(e,1350);
				
				this.yPos=-1300;this.xPos=azar(725); 
				
			}
		
		}
		if (this.yPos>600)
		{	
			
			this.yPos=-1300;this.xPos=azar(725); 
		}
	}
	
}

function d() {
    explosion.pintar(r);	
}

//---------------------------------------------------------------------------
// MULTIJUGADOR: jugador remoto (el que controla el otro ordenador)
// Se guarda su ultima posicion recibida y se dibuja en cada vuelta del bucle.
var remoto = {conectado:false, x:0, y:480, sy:0, frame:0, xPintada:null};

// Actualiza los datos del remoto con lo recibido por WebSocket (lo llama WebSocket.js)
function actualizarRemoto(datos)
{
	remoto.conectado=true;
	remoto.x=datos.x;
	remoto.y=datos.y;
	remoto.sy=datos.sy;//fila del sprite: 0 derecha, 112 izquierda
	remoto.frame=datos.frame;//fotograma de la animacion
}

function blastRemoto(datos)
{
	blast(datos.x, datos.y);
}

// Dibuja al remoto. Si se ha movido, primero borra el rastro de la posicion anterior
function pintarRemoto()
{
	if (!remoto.conectado) return;
	if (remoto.xPintada!==null && remoto.xPintada!==remoto.x)
	{
		ctx.drawImage(bg, remoto.xPintada,480,sw,sh,remoto.xPintada,480,dw,dh);
	}
	ctx.save();
	ctx.filter="hue-rotate(150deg)";//otro color para distinguirlo del jugador local
	ctx.drawImage(playerSprite,sw*remoto.frame,remoto.sy,sw,sh,remoto.x,remoto.y,dw,dh);
	ctx.restore();
	remoto.xPintada=remoto.x;
}

// El otro jugador se ha desconectado: lo borramos de la pantalla
function quitarRemoto()
{
	if (remoto.xPintada!==null)
	{
		ctx.drawImage(bg, remoto.xPintada,480,sw,sh,remoto.xPintada,480,dw,dh);
	}
	remoto.conectado=false;
	remoto.xPintada=null;
}

function c()
{			
	if (!colision)
	{
		for(t=0;t<15;t++)
		{	
			v[t].pintar();	
		}
		for(t=0;t<5;t++)
		{
			moneda[t].pintar();
		}
	}			
}

function a() {			
	ctx.drawImage(bg, p,480,sw,sh,p,480,dw,dh);// el fondo es de 800*600
	p-=paso;
	if (p<0){p=0;}
	frameIndex=(frameIndex+1) % totalFrames;
	sx=sw*frameIndex;
	ctx.drawImage(playerSprite,sx,sy,sw,sh,dx+p,dy,dw,dh);
	enviarPosicion();//MULTIJUGADOR: avisamos al servidor de la nueva posicion
}

function b() {		
	ctx.drawImage(bg, p,480,sw,sh,p,480,dw,dh);// el fondo es de 800*600
	p+=paso;
	if (p>729){p=729;}//800-71 ancho del fondo menos ancho del sprite
	frameIndex=(frameIndex+1) % totalFrames;
	sx=sw*frameIndex;
	ctx.drawImage(playerSprite,sx,sy,sw,sh,dx+p,dy,dw,dh);	
	enviarPosicion();//MULTIJUGADOR: avisamos al servidor de la nueva posicion
}

function checkKeyPressed(e) 
{
	if ((e.keyCode == "37") && (!colision))
	{
		sy=112;				
		setTimeout(a,0);												
		setTimeout(a,100);							
		setTimeout(a,200);						
		setTimeout(a,300);	
		//setTimeout(a,400);	
		
	}
	if ((e.keyCode == "39") && (!colision))
	{
		sy=0;
		setTimeout(b,0);												
		setTimeout(b,100);							
		setTimeout(b,200);						
		setTimeout(b,300);	
		//setTimeout(b,400);	
	}
			
}	
refreshIntervalId =setInterval(c,50);
//MULTIJUGADOR: el remoto se repinta con su propio temporizador (independiente de c())
setInterval(pintarRemoto,50);

//---------------------------------------------------------------------------
function crearBombas(n)
{
	for(t=0;t<n;t++)
	{			
		v[t]=new bomba(azar(725),-100*t);
	}
	for(t=0;t<n;t++)
	{	
		moneda[t]=new coin(azar(700),-2100-100*t);
	}

}

window.onload = function() {
   //Recibimos el elemento canvas
   ctx = cargaContextoCanvas('micanvas');			 	    
   if(ctx) {			
		//indico la URL de la imagen
		playerSprite.src = 'Assets/Imgs/Sprites/sprite.png';
		//defino el evento onload del objeto imagen
		playerSprite.onload = function() {
			//incluyo la imagen en el canvas
			 ctx.drawImage(playerSprite,sx,sy,sw,sh,dx,dy,dw,dh);
	
		    bg.onload = function() 
		    {
		    	ctx.drawImage(bg, 0, 0);
		    }
		    bg.src = "Assets/Imgs/Enviroment/fondo.jpg";		

		    //-----------------------------
        
		    crearBombas(10);
		
	    }   
    }
}