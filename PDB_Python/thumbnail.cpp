// vim: tabstop=2 expandtab shiftwidth=2 softtabstop=2

#include <Python.h>
#include <modsupport.h>
#include <iostream>

#if PY_MAJOR_VERSION < 3
  #error !!
#endif

class thumbnail {
public:
  // constructor initialises the module with a table of methods.
  thumbnail() {
    static PyMethodDef methods[] = {
      {"make_thumbnail", &make_thumbnail, METH_VARARGS, "make a thumbnail image of a molecule"},
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
std::cout << "hello\n";
    Py_buffer image_buf = {0};
    Py_buffer pos_buf = {0};
    Py_buffer radii_buf = {0};
    PyArg_ParseTuple(args, "z*z*z*", &image_buf, &pos_buf, &radii_buf);
    std::cout << pos_buf.buf << "\n";
    float *pos = (float*)pos_buf.buf;
    float *radii = (float*)radii_buf.buf;
    size_t natoms = (size_t)radii_buf.len/4;
    for (size_t i = 0; i != natoms; ++i) {
      printf("%f %f %f %f\n", pos[i*3+0], pos[i*3+1], pos[i*3+2], radii[i]);
    }
    Py_RETURN_NONE;
  }
};


PyMODINIT_FUNC
PyInit_thumbnail() {
  thumbnail *mod = new thumbnail();
  return mod->get_module();
}

