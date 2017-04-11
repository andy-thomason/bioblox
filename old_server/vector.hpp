
#ifndef VECTOR_INCLUDED
#define VECTOR_INCLUDED


struct vector3 {
  float x, y, z;
  
  vector3() : x(0), y(0), z(0) {}
  vector3(float x, float y, float z) : x(x), y(y), z(z) {}

  float length() const { return std::sqrt(x*x + y*y + z*z); }

  static vector3 lerp(vector3 a, vector3 b, float lambda) {
    float rl = 1.0f - lambda;
    return vector3(a.x * rl + b.x * lambda, a.y * rl + b.y * lambda, a.z * rl + b.z * lambda);
  }
  
  static vector3 max(vector3 a, vector3 b) {
    return vector3(std::max(a.x, b.x), std::max(a.y, b.y), std::max(a.z, b.z));
  }

  static vector3 min(vector3 a, vector3 b) {
    return vector3(std::min(a.x, b.x), std::min(a.y, b.y), std::min(a.z, b.z));
  }

  vector3 operator *(float v) { return vector3(x*v, y*v, z*v); }
  
  vector3 normalised() { float rl = 1.0f / length(); return *this * rl; }
  
  void normalise() {
    float len = length();
    if (len >= 0.00001f) {
      float rl = 1.0f / len;
      x *= rl;
      y *= rl;
      z *= rl;
    }
  }
};

struct colour {
  float r, g, b, a;
  
  colour() : r(1), g(1), b(1), a(1) {}
  colour(float r, float g, float b, float a) : r(r), g(g), b(b), a(a) {}

  static colour lerp(colour a, colour b, float lambda) {
    float rl = 1.0f - lambda;
    return colour(a.r * rl + b.r * lambda, a.g * rl + b.g * lambda, a.b * rl + b.b * lambda, a.a * rl + b.a * lambda);
  }
};


#endif
