function wormol() {
  var gl;

  // these values help us to avoid creating new objects every time
  var camera_to_perspective = mat4.create();
  var world_to_camera = mat4.create();
  var model_to_camera = mat4.create();
  var model_to_perspective = mat4.create();
  var model_to_world = mat4.create();
  var scene = {};
  var scene_params = {};
  var vel_x = 0;
  var vel_y = 0;
  var mouse = { down: false, x: 0, y: 0, button: 0 };
  var last = { down: false, x: 0, y: 0, button: 0 };

  function init_webgl(canvas) {
    try {
      gl = canvas.getContext("experimental-webgl");
      gl.viewportWidth = canvas.width;
      gl.viewportHeight = canvas.height;
    } catch (e) {
      console.log("throw!");
    }
    if (!gl) {
      alert("Could not initialise WebGL, sorry :-(");
    }
  }

  function make_shader(gl, id) {
    var shaderScript = document.getElementById(id);
    if (!shaderScript) {
      return null;
    }

    var str = "";
    var k = shaderScript.firstChild;
    while (k) {
      if (k.nodeType == 3) {
        str += k.textContent;
      }
      k = k.nextSibling;
    }

    var shader;
    if (shaderScript.type == "x-shader/x-fragment") {
      shader = gl.createShader(gl.FRAGMENT_SHADER);
    } else if (shaderScript.type == "x-shader/x-vertex") {
      shader = gl.createShader(gl.VERTEX_SHADER);
    } else {
      return null;
    }

    gl.shaderSource(shader, str);
    gl.compileShader(shader);

    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
      alert(gl.getShaderInfoLog(shader));
      return null;
    }

    return shader;
  }

  function make_program(vs, fs) {
    program = gl.createProgram();
    gl.attachShader(program, make_shader(gl, vs));
    gl.attachShader(program, make_shader(gl, fs));
    gl.linkProgram(program);

    if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
      alert("could not compile " + vs + " or " + fs);
    }

    program.model_to_camera = gl.getUniformLocation(program, "model_to_camera");
    program.model_to_perspective = gl.getUniformLocation(program, "model_to_perspective");
    return program;
  }


  function draw_scene(scene, scene_params) {
    if (scene == undefined) return;

    if (!scene_params.active_camera) {
      for (s in scene) {
        node = scene[s];
        if (node.optics) {
          scene_params.active_camera = node;
        }
      }
    }

    active_camera = scene_params.active_camera;
    optics = active_camera.optics;

    gl.viewport(0, 0, gl.viewportWidth, gl.viewportHeight);
    gl.clearColor(.2, .2, .2, 1);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.enable(gl.DEPTH_TEST);

    if (optics.yfov) {
      mat4.perspective(optics.yfov, gl.viewportWidth / gl.viewportHeight, optics.znear, optics.zfar, camera_to_perspective);
    } else {
      mat4.ortho(camera_to_perspective);
    }
    //console.log("camera_to_perspective=" + mat4.str(camera_to_perspective));

    mat4.inverse(active_camera.node_to_parent, world_to_camera);
    //console.log("world_to_camera=" + mat4.str(world_to_camera));

    for (s in scene) {
      node = scene[s];
      if (node.geometry) {
        //console.log(s + ": " + node.geometry.components.length);
        model_to_world = node.node_to_parent;
        //console.log("model_to_world=" + mat4.str(model_to_world));

        mat4.set(world_to_camera, model_to_camera);
        mat4.multiply(model_to_world, model_to_camera, model_to_camera);
        //console.log("model_to_camera=" + mat4.str(model_to_camera));

        mat4.set(model_to_camera, model_to_perspective);
        mat4.multiply(camera_to_perspective, model_to_perspective, model_to_perspective);
        //console.log("model_to_perspective=" + mat4.str(model_to_perspective));

        //tmp = [0, 0, 0, 0];
        //mat4.multiplyVec4(camera_to_perspective, [0, 0, 0, 1], tmp);
        //console.log("t=" + tmp);

        program = node.geometry.program;
        gl.useProgram(program);
        gl.uniformMatrix4fv(program.model_to_perspective, false, model_to_perspective);
        gl.uniformMatrix4fv(program.model_to_camera, false, model_to_camera);

        components = node.geometry.components;
        for (c in components) {
          component = components[c];
          //console.log("c: " + component.vbo + " i: " + component.ibo);

          gl.bindBuffer(gl.ARRAY_BUFFER, component.vbo);
          gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, component.ibo);

          for (a in component.attrs) {
            attr = component.attrs[a];
            attr_index = gl.getAttribLocation(program, attr.semantic);
            //console.log("attr[" + attr_index + "] n=" + attr.semantic + " s=" + attr.size + " t=" + attr.type + " n=" + attr.normalized + " st=" + component.stride + " o=" + attr.offset);
            gl.vertexAttribPointer(attr_index, attr.size, attr.type, attr.normalized, component.stride, attr.offset);
            gl.enableVertexAttribArray(attr_index);
            attr.index = attr_index;
          }

          gl.drawElements(gl.TRIANGLES, component.num_indices, gl.UNSIGNED_SHORT, 0);

          for (a in component.attrs) {
            attr = component.attrs[a];
            attr_index = gl.getAttribLocation(program, attr.semantic);
            gl.disableVertexAttribArray(attr_index);
          }
        }
      }
    }
  }

  // from glMol: (note, jmol has a much larger group of colours
  colours = {
    ' H': 0xcccccc, ' C': 0xaaaaaa, ' O': 0xcc0000, ' N': 0x0000cc, ' S': 0xcccc00,
    ' P': 0x6622cc, ' F': 0x00cc00, 'CL': 0x00cc00, 'BR': 0x882200, ' I': 0x6600aa,
    'FE': 0xcc6600, 'CA': 0x8888aa
  };

  // Reference: glMol / A. Bondi, J. Phys. Chem., 1964, 68, 441.
  radii = {
    ' H': 1.2, 'LI': 1.82, 'NA': 2.27, ' K': 2.75, ' C': 1.7, ' N': 1.55, ' O': 1.52,
    ' F': 1.47, ' P': 1.80, ' S': 1.80, 'CL': 1.75, 'BR': 1.85, 'SE': 1.90,
    'ZN': 1.39, 'CU': 1.4, 'NI': 1.63
  };

  // parse an ATOM or HETATM line from the PDB file
  function parse_atom(atoms, line) {
    /*
      recname	0	5	A6	 a literal "ATOM  " (note two trailing spaces).
      serial	6	10	I5	 atom serial number, e.g. "   86". See below for details.
      11	11	1X	space
      atom	12	15	A4	 Atom role name, e.g. " CG1;". See below for details.
      altLoc	16	16	A1	 atom variant, officially called the "alternate location indicator". This is usually " " for atoms with well-defined positions, as in this case, but sometimes "A", "B", etc. See below for details.
      resName	17	19	A3	 amino acid abbreviation, e.g. "ARG". See below for details.
      20	20	1X	space
      chainID	21	21	A1	 chain ID, usually " ", but often "A", "B", etc, for multichain entries. See below for details.
      Seqno	22	26	A5	 residue sequence number (I4) and insertion code (A1), e.g. "  11 " or " 256C". See below for details.
      27	29	3X	three spaces
      x	30	37	F8.3	atom X coordinate
      y	38	45	F8.3	atom Y coordinate
      z	46	53	F8.3	atom Z coordinate
      occupancy	54	59	F6.2	 atom occupancy, usually "  1.00". The sum of atom occupancies for all variants in field 4 generally add to 1.0.
      tempFactor	60	65	F6.2	 B value or temperature factor, e.g. " 17.72". (I don't use this value, so have nothing to add; see the ATOM record specification discussion of B factors, etc. -- rgr, 8-Oct-96.)
    */
    elem = line.substring(12, 14);
    colour = colours[elem];
    radius = radii[elem];
    colour = colour ? colour : 0x808080;
    radius = radius ? radius : 1.5;
    x = parseFloat(line.substring(30, 38));
    y = parseFloat(line.substring(38, 46));
    z = parseFloat(line.substring(46, 54));

    atom = {
      elem: elem, colour: colour, radius: radius, x: x, y: y, z: z
    };
    atoms.push(atom);
  }

  // convert the atoms[] array into renderable geometry
  function build_geometry(atoms) {
    num_atoms = atoms.length;

    components = []
    for (i = 0; i != num_atoms;) {
      imax = Math.min(num_atoms, i + 5000);
      components.push(build_component(atoms, i, imax));
      i = imax;
    }

    molecule_to_world = mat4.create();
    mat4.identity(molecule_to_world);

    camera_to_world = mat4.create();
    mat4.identity(camera_to_world);
    mat4.translate(camera_to_world, [0, 0, 200]);

    // the scene contains many models with their own transforms
    scene = {
      camera: {
        node_to_parent: camera_to_world,
        optics: {
          yfov: 45,
          znear: 0.1,
          zfar: 1000.0,
        }
      },
      molecule: {
        node_to_parent: molecule_to_world,
        geometry: {
          program: make_program("shader-vs", "shader-fs"),
          components: components
        }
      }
    }
    return scene;
  }

  c = 1.618033988749895;
  // icosphere (http://en.wikipedia.org/wiki/Icosahedron#Cartesian_coordinates)
  vproto = [-1, c, 0, 1, c, 0, -1, -c, 0, 1, -c, 0, 0, -1, c, 0, 1, c, 0, -1, -c, 0, 1, -c, c, 0, -1, c, 0, 1, -c, 0, -1, -c, 0, 1];
  iproto = [0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8, 3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1];

  // a component has a maximum of 65536 indices, so we must make many
  function build_component(atoms, imin, imax) {
    //console.log(imin + " ... " + imax);
    idiff = imax - imin;
    nscale = 0.5257311121191336;
    vscale = atom.radius * nscale;
    rscale = 1.0 / 255;

    vplen3 = vproto.length / 3;
    iplen = iproto.length;
    vertices = new Float32Array(idiff * vplen3 * 9);
    indices = new Uint16Array(idiff * iplen);
    dest = 0;
    idest = 0;

    //console.log("iplen=" + iplen);

    // build the vertices
    for (i = imin; i != imax; ++i) {
      atom = atoms[i];
      x = atom.x;
      y = atom.y;
      z = atom.z;
      r = ((atom.colour & 0xff0000) >> 16) * rscale;
      g = ((atom.colour & 0xff00) >> 8) * rscale;
      b = (atom.colour & 0xff) * rscale;
      vpsrc = 0;
      for (j = 0; j != vplen3; ++j) {
        vx = vproto[vpsrc + 0];
        vy = vproto[vpsrc + 1];
        vz = vproto[vpsrc + 2];
        vertices[dest + 0] = x + vx * vscale;
        vertices[dest + 1] = y + vy * vscale;
        vertices[dest + 2] = z + vz * vscale;
        vertices[dest + 3] = vx * nscale;
        vertices[dest + 4] = vy * nscale;
        vertices[dest + 5] = vz * nscale;
        vertices[dest + 6] = r;
        vertices[dest + 7] = g;
        vertices[dest + 8] = b;
        vpsrc += 3;
        dest += 9;
      }
    }

    // build the index
    ibase = 0;
    for (i = 0; i != idiff; ++i) {
      for (j = 0; j != iplen; ++j) {
        indices[idest++] = ibase + iproto[j];
      }
      ibase += vplen3;
    }

    //console.log(atoms.length + " atoms");
    //console.log(dest + ": " + vertices.length);
    //console.log(idest + ": " + indices.length);

    vbo = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, vbo);
    gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

    ibo = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, ibo);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

    /*for (i = 0; i != 10; ++i) {
      console.log(i + ": " + indices[i]);
    }*/

    // a component is 
    component = {
      vbo: vbo,
      ibo: ibo,
      num_indices: indices.length,
      stride: 36,
      attrs: [
        { semantic: "POSITION", size: 3, type: gl.FLOAT, normalized: false, offset: 0 },
        { semantic: "NORMAL", size: 3, type: gl.FLOAT, normalized: false, offset: 12 },
        { semantic: "COLOR", size: 3, type: gl.FLOAT, normalized: false, offset: 24 },
      ]
    }

    return component;
  }

  function make_pdb_scene(text) {
    atoms = []
    text = text.split('\n');
    for (i = 0; i != text.length; ++i) {
      line = text[i];
      //console.log(line)
      switch (line.substr(0, 6)) {
        case 'ATOM  ':
        case 'HETATM': {
          parse_atom(atoms, line);
        } break;
      }
    }

    scene = build_geometry(atoms);
    console.log("done loading");
    return scene;
  }

  tick = function () {
    if (mouse.down && last.down && mouse.button == 0 && scene_params.active_camera) {
      dx = mouse.x - last.x;
      dy = mouse.y - last.y;

      vel_x = 0.9 * vel_x + 0.1 * dy * -0.01;
      vel_y = 0.9 * vel_y + 0.1 * dx * -0.01;
    }

    last.down = mouse.down;
    last.x = mouse.x;
    last.y = mouse.y;
    last.button = mouse.button;

    //console.log(vel_x, vel_y);
    spin_camera(scene_params, vel_x, vel_y);
    draw_scene(scene, scene_params);
    vel_x *= 0.9;
    vel_y *= 0.9;
  }

  function wormol_readystatechange() {
    if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
      scene = make_pdb_scene(xmlhttp.responseText);
      draw_scene(scene, scene_params);
      setInterval(tick, 33);
    }
  }

  function loadPDB(url) {
    xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = wormol_readystatechange;
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
  }

  function spin_camera(scene_params, rx, ry) {
    //console.log("scene_params: ", scene_params);
    if (scene_params.active_camera) {
      node_to_parent = scene_params.active_camera.node_to_parent;
      mat4.translate(node_to_parent, [0, 0, -200]);
      mat4.rotate(node_to_parent, rx, [1, 0, 0]);
      mat4.rotate(node_to_parent, ry, [0, 1, 0]);
      mat4.translate(node_to_parent, [0, 0, 200]);
    }
  }

  init_webgl(document.getElementById("viewport"));

  //loadPDB("http://www.ebi.ac.uk/msd-srv/capri/round1/brk/capri_01.brk");
  loadPDB("assets/capri_01.brk");

  // public interface
  this.onmousedown = function (event) {
    event = window.event || event;
    mouse.down = true;
    mouse.x = event.clientX;
    mouse.y = event.clientY;
  }

  this.onmouseup = function (event) {
    event = window.event || event;
    mouse.down = false;
  }

  this.onmousemove = function (event) {
    event = window.event || event;
    mouse.x = event.clientX;
    mouse.y = event.clientY;
  }

  this.oncontextmenu = function () {
    return false;
  }

}
