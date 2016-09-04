var gl = GL.create();

var vertexShader;
var fragmentShader;


window.onload = function() {
	loadVertexShader(function() {
		loadFragmentShader(function() {
			main();
		});
	});
}

function main() {
	var ratio = window.devicePixelRatio || 1;
	var onepiece = document.getElementById('onepiece');
	var grid = document.getElementById('grid');
	var fibre = document.getElementById('fibre');
	var tiles = document.getElementById('tiles');
	var center = [0.0, 0.0];
	var width, height;

	function onresize() {
		width = innerWidth/3;
		height = innerHeight/3;
		gl.canvas.width = width * ratio;
		gl.canvas.height = height * ratio;
		gl.canvas.style.width = innerWidth + 'px';
		gl.canvas.style.height = innerHeight + 'px';
		gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
		gl.matrixMode(gl.PROJECTION);
		gl.loadIdentity();
		gl.perspective(45, gl.canvas.width / gl.canvas.height, 0.01, 100);
		gl.matrixMode(gl.MODELVIEW);
		draw(center);
	}
	document.body.appendChild(gl.canvas);
	gl.clearColor(0, 0, 0, 1);

	var textureShader = new GL.Shader(vertexShader, fragmentShader);
	var spaceTexture = GL.Texture.fromImage(onepiece, {
		minFilter: gl.NEAREST,
		format: gl.RGB,
		wrap: gl.REPEAT
	});
	var discTexture = GL.Texture.fromImage(fibre, {
		minFilter: gl.NEAREST,
		format: gl.RGB,
		wrap: gl.REPEAT
	});
	var t = new GL.Mesh.plane();

	onresize();
	gl.onmousemove = function(e) {
		center[0] = e.x / innerWidth * 2.0 - 1.0;
		center[1] = (innerHeight - e.y) / innerHeight * 2.0 - 1.0;
		center[0] *= width / height;
	};
	gl.ondraw = function() {
		draw(center);
	}
	gl.animate();

	function draw(center) {
		spaceTexture.bind(0);
		discTexture.bind(1);
		textureShader.uniforms({
			space: 0,
			disc: 1,
			resolution: [innerWidth * ratio / 3, innerHeight * ratio / 3],
			iGlobalTime: performance.now(),
			center: center
		});
		textureShader.draw(t);
	}
}

function loadVertexShader(callback) {							
	xhr = new XMLHttpRequest();
	xhr.open('GET', 'renders/space.vs', true);
	xhr.onreadystatechange = function() {
		if (xhr.readyState === 4 && xhr.status === 200) {
			vertexShader = xhr.responseText;
			if (callback) {
				callback();
			}
		}
	};
	xhr.send(null);		    	
}
function loadFragmentShader(callback) {
	xhr = new XMLHttpRequest();
	xhr.open('GET', 'renders/space.fs', true);
	xhr.onreadystatechange = function() {
		if (xhr.readyState === 4 && xhr.status === 200) {
			fragmentShader = xhr.responseText;
			if (callback) {
				callback();
			}
		}
	};
	xhr.send(null);
}
