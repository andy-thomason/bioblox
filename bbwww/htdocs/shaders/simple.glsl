precision mediump float;

#ifdef VERTEX_SHADER
	attribute vec3 POSITION;

	void main(void) {
	  gl_Position = vec4(POSITION, 1.0);
	}
#endif

#ifdef FRAGMENT_SHADER
	void main(void) {
	  gl_FragColor = vec4(1, 0, 0, 1);
	}
#endif
