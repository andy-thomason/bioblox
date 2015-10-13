/// @file bioblox.js
/// Bioblox game

// This is necessary to use modern Javascript classes.
"use strict";

/// A geometry component
class Bioblox extends Scene {
  constructor() {
    super(function (scene) {
      var gl = scene.gl;
      var indices = new Uint16Array([0, 1, 2]);
      var vertices = new Float32Array([
        -1, -1, -0.5, 0, 0, 1, 1, 0, 0,
        0, 1, -0.5, 0, 0, 1, 1, 0, 0,
          1, -1, -0.5, 0, 0, 1, 1, 0, 0
      ]);

      var attrs = {
        POSITION: { size: 3, type: gl.FLOAT, normalized: false, offset:  0 },
        NORMAL:   { size: 3, type: gl.FLOAT, normalized: false, offset: 12 },
        COLOR:    { size: 3, type: gl.FLOAT, normalized: false, offset: 24 },
      };

      var components = [new Component(scene.gl, vertices, indices, 36, attrs)];
      var shader = new Shader(scene.gl, "shaders/simple.glsl");
      var color = new glMatrixArrayType([1, 0, 0, 1]);
      var material = new Material(shader, { color: color });

      return {
        camera: new CameraNode(45, 0.1, 1000.0).translate([0, 0, 5]),
        molecule: new GeometryNode(components, material),
      }
    });
  }
}
