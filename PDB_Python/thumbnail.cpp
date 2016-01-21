// vim: tabstop=2 expandtab shiftwidth=2 softtabstop=2

#include <Python.h>
#include <tupleobject.h>

#include <modsupport.h>
#include <iostream>
#include <cmath>
#include <vector>

#if PY_MAJOR_VERSION < 3
  #error must be built with Python 3
#endif

#include "molecule_builder.hpp"

class thumbnail {
public:
  // constructor initialises the module with a table of methods.
  thumbnail() {
    static PyMethodDef methods[] = {
      {"make_thumbnail", &make_thumbnail, METH_VARARGS, "make a thumbnail image of a molecule"},
      {"make_mesh", &make_mesh, METH_VARARGS, "make a mesh for a molecule"},
      {0, 0, 0, 0}
    };
    static struct PyModuleDef moduledef = {
      PyModuleDef_HEAD_INIT,
      "thumbnail",
      NULL,
      -1,
      methods,
      NULL,
      NULL,
      NULL,
      NULL
    };
    module = PyModule_Create(&moduledef);
  }

  PyObject *get_module() { return module; }

private:
  /// This python module
  PyObject *module = nullptr;

  // read a string parameter
  static PyObject *make_thumbnail(PyObject *self, PyObject *args) {
    Py_buffer image_buf = {0};
    Py_buffer pos_buf = {0};
    Py_buffer radii_buf = {0};
    Py_buffer chain_buf = {0};

    int width = 0;
    int height = 0;
    PyArg_ParseTuple(args, "z*z*z*z*ii", &image_buf, &pos_buf, &radii_buf, &chain_buf, &width, &height);
    std::cout << pos_buf.buf << "\n";
    float *pos = (float*)pos_buf.buf;
    float *radii = (float*)radii_buf.buf;
    std::uint8_t *image = (std::uint8_t *)image_buf.buf;
    std::uint8_t *chain = (std::uint8_t *)chain_buf.buf;
    size_t natoms = (size_t)radii_buf.len/4;
    std::cout << image_buf.len << "\n";
    memset(image, 0xff, width*height*3);

    static const std::uint8_t colours[] = {
      0xdf, 0x60, 0x60,  0xdf, 0xdf, 0x60,  0x60, 0x60, 0xdf, 0x60, 0xdf, 0xdf,
      0xdf, 0x40, 0x40,  0xdf, 0xdf, 0x40,  0x40, 0x40, 0xdf, 0x40, 0xdf, 0xdf,
      0xdf, 0xa0, 0xa0,  0xdf, 0xdf, 0xa0,  0xa0, 0xa0, 0xdf, 0xa0, 0xdf, 0xdf,
      0xdf, 0x20, 0x20,  0xdf, 0xdf, 0x20,  0x20, 0x20, 0xdf, 0x20, 0xdf, 0xdf,
    };
    std::vector<float> zbuf(width*height);

    if (natoms != 0) {
      float min_x = pos[0], max_x = min_x;
      float min_y = pos[1], max_y = min_y;
      float min_z = pos[2];
      for (size_t i = 0; i != natoms; ++i) {
        float r = radii[i];
        min_x = std::min(min_x, pos[i*3+0] - r*2);
        min_y = std::min(min_y, pos[i*3+1] - r*2);
        min_z = std::min(min_z, pos[i*3+2] - r*2);
        max_x = std::max(max_x, pos[i*3+0] + r*2);
        max_y = std::max(max_y, pos[i*3+1] + r*2);
      }

      float border = 0.20f;
      float scale = std::min(width, height) / std::max(max_x-min_x, max_y - min_y);
      float rscale = 1.0f / scale;
      float ox = -min_x * scale;
      float oy = -min_y * scale;
      for (size_t i = 0; i != natoms; ++i) {
        //printf("%f %f %f %f\n", pos[i*3+0], pos[i*3+1], pos[i*3+2], radii[i]);
        int colour = (chain[i] & 0x0f) * 3;
        float cx = pos[i*3+0];
        float cy = pos[i*3+1];
        float cz = pos[i*3+2] - min_z;
        float r = radii[i];
        float rb = r + border;
        int x0 = (int)((cx - rb) * scale + ox);
        int y0 = (int)((cy - rb) * scale + oy);
        int x1 = (int)((cx + rb) * scale + ox);
        int y1 = (int)((cy + rb) * scale + oy);
        float col_r = colours[colour + 0];
        float col_g = colours[colour + 1];
        float col_b = colours[colour + 2];
        float rcpr = 1.0f / r;

        for (int iy = y0; iy <= y1; ++iy) {
          float y = (iy - oy) * rscale - cy;
          if (iy >= 0 && iy < height) {
            for (int ix = x0; ix <= x1; ++ix) {
              if (ix >= 0 && ix < width) {
                float x = (ix - ox) * rscale - cx;
                if (x*x + y*y <= r*r) {
                  float z = std::sqrt(r*r - (x*x + y*y));
                  float nx = x * rcpr;
                  float ny = y * rcpr;
                  float nz = z * rcpr;
                  float diff = std::max(0.0f, nx * 0.5f + ny * 0.1f + nz * 0.6f) * 0.5f + 0.5f;
                  if (z + cz > zbuf[ix+iy*width]) {
                    zbuf[ix+iy*width] = z + cz;
                    image[(ix+iy*width)*3+0] = col_r * diff;
                    image[(ix+iy*width)*3+1] = col_g * diff;
                    image[(ix+iy*width)*3+2] = col_b * diff;
                  }
                } else if (x*x + y*y <= rb*rb) {
                  if (zbuf[ix+iy*width] == 0) {
                    //zbuf[ix+iy*width] = z + cz;
                    image[(ix+iy*width)*3+0] = 0;
                    image[(ix+iy*width)*3+1] = 0;
                    image[(ix+iy*width)*3+2] = 0;
                  }
                }
              }
            }
          }
        }
      }
    }
    Py_RETURN_NONE;
  }
  
  static PyObject *make_mesh(PyObject *self, PyObject *args) {
    Py_buffer pos_buf = {0};
    Py_buffer radii_buf = {0};
    Py_buffer chain_buf = {0};

    int resolution = 0;
    PyArg_ParseTuple(args, "s*s*s*i", &pos_buf, &radii_buf, &chain_buf, &resolution);

    molecule_builder mb(pos_buf.len/12, (const vector3*)pos_buf.buf, (const float*)radii_buf.buf, (const char*)chain_buf.buf, resolution / 100.0f);

    return PyBytes_FromStringAndSize((const char*)mb.image().data(), mb.image().size()*4);
  }
};


PyMODINIT_FUNC
PyInit_thumbnail() {
  thumbnail *mod = new thumbnail();
  return mod->get_module();
}

