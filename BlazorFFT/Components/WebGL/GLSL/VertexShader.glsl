precision mediump float;

attribute vec3 vertPosition;
attribute vec3 vertColor;

varying vec3 fragColor;

uniform mat4 mWorld;
uniform mat4 mView;
uniform mat4 mProj;

void main(){
	fragColor = vertColor;
	gl_Position = mProj * mView * mWorld * vec4(vertPosition, 1.0);
}