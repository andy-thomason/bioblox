
#include <Python.h>
#include <modsupport.h>
#include <iostream>

class extension {
public:
  // constructor initialises the module with a table of methods.
  extension() {
    static PyMethodDef methods[] = {
      {"hello", &hello, METH_VARARGS, "say hello"},
      {"add", &add, METH_VARARGS, "add two ints"},
      {"callback", &callback, METH_VARARGS, "call the argument"},
      {0, 0, 0, 0}
    };
    module = Py_InitModule("extension", methods);

  }

private:
  /// This python module
  PyObject *module = nullptr;

  // read a string parameter
  static PyObject *hello(PyObject *self, PyObject *args) {
    // note we never declare a variable without clearing it!
    char *str = nullptr;
    PyArg_ParseTuple(args, "s", &str);
    std::cout << "hello " << str << "\n";
    Py_RETURN_NONE;
  }

  // manipulate numbers and create a python number object
  static PyObject *add(PyObject *self, PyObject *args) {
    int a = 0, b = 0;
    PyArg_ParseTuple(args, "ii", &a, &b);
    return PyInt_FromLong(a + b);
  }

  // call a python function.
  static PyObject *callback(PyObject *self, PyObject *args) {
    PyObject *func = nullptr;
    PyArg_ParseTuple(args, "O", &func);

    if (PyCallable_Check(func)) {
      // build a tuple with one integer entry.
      PyObject *arglist = Py_BuildValue("(i)", 1234);

      // call the python function.
      PyObject_CallObject(func, arglist);

      // remove the arg list
      Py_DECREF(arglist);
    }
    Py_RETURN_NONE;
  }
};


PyMODINIT_FUNC
initextension() {
  extension *e = new extension();
}

