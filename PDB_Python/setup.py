from distutils.core import setup, Extension

thumbnail = Extension('thumbnail', sources = ['thumbnail.cpp'], extra_compile_args=['-std=c++11'])

setup (name = 'PackageName', version = '1.0', description = 'thumbnail generator', ext_modules = [thumbnail])
