
#include <algorithm>
#include "glslmath/include/marching_cubes.hpp"

/*
  static void make_molecule(float grid_spacing) {
    const float rgs = 1.0f / grid_spacing;

    // Create a 3D array for each molecule
    Vector3 min = pos[0];
    Vector3 max = min;
    float max_r = 0.0f;
    for (int j = 0; j != pos.Length; ++j)
    {
      //Vector3 r = new Vector3(atom_radii[j], atom_radii[j], atom_radii[j]) + 1.0f;
      float r = atom_radii[j];
      max_r = Mathf.Max(max_r, r);
      min = Vector3.Min(min, pos[j]);
      max = Vector3.Max(max, pos[j]);
    }

    int irgs = Mathf.CeilToInt (max_r * rgs) + 1;
    int x0 = std::floor (min.x * rgs) - irgs;
    int y0 = std::floor (min.y * rgs) - irgs;
    int z0 = std::floor (min.z * rgs) - irgs;
    int x1 = Mathf.CeilToInt (max.x * rgs) + irgs;
    int y1 = Mathf.CeilToInt (max.y * rgs) + irgs;
    int z1 = Mathf.CeilToInt (max.z * rgs) + irgs;

    int xdim = x1-x0+1, ydim = y1-y0+1, zdim = z1-z0+1;

    // Array contains values (-0.5..>1) positive means inside molecule.
    // Normals are drived from values.
    // Colours shaded by ambient occlusion (TODO). 
    float[] mc_values = new float[xdim * ydim * zdim];
    Vector3[] mc_normals = new Vector3[xdim * ydim * zdim];
    Color[] mc_colours = new Color[xdim * ydim * zdim];
    //float diff = 0.125f, rec = 2.0f / diff;
    for (int i = 0; i != xdim * ydim * zdim; ++i) {
      mc_values[i] = -0.5f;
      mc_colours[i] = new Color(1, 1, 1, 1);
    }

    // For each atom add in the values and normals surrounding
    // the centre up to a reasonable radius.
    int acmax = pos.Length;
    DateTime start_time = DateTime.Now;
    for (int ac = 0; ac != acmax; ++ac) {
      Vector3 c = pos[ac];
      float r = atom_radii[ac] * 0.8f * rgs;
      Color colour = atom_colours[ac];
      float cx = c.x * rgs;
      float cy = c.y * rgs;
      float cz = c.z * rgs;

      // define a box around the atom.
      int cix = std::floor(c.x * rgs);
      int ciy = std::floor(c.y * rgs);
      int ciz = std::floor(c.z * rgs);
      int xmin = Mathf.Max(x0, cix-irgs);
      int ymin = Mathf.Max(y0, ciy-irgs);
      int zmin = Mathf.Max(z0, ciz-irgs);
      int xmax = Mathf.Max(x1, cix+irgs);
      int ymax = Mathf.Max(y1, ciy+irgs);
      int zmax = Mathf.Max(z1, ciz+irgs);
      float fk = Mathf.Log(0.5f) / (r * r);
      float fkk = -fk * 0.5f; // 0.5 is a magic number!
      bool wcol = colour != Color.white;

      for (int z = zmin; z != zmax; ++z) {
        float fdz = z - cz;
        for (int y = ymin; y != ymax; ++y) {
          float fdy = y - cy;
          int idx = ((z-z0) * ydim + (y-y0)) * xdim + (xmin-x0);
          float d2_base = fdy*fdy + fdz*fdz;
          for (int x = xmin; x != xmax; ++x) {
            float fdx = x - cx;
            float d2 = fdx*fdx + d2_base;
            float v = fkk * d2;
            if (v < 1) {
              // a little like exp(-v)
              float val = (2 * v - 3) * v * v + 1;
              mc_values[idx] += val;
              float rcp = val / Mathf.Sqrt(d2);
              mc_normals[idx].x += fdx * rcp;
              mc_normals[idx].y += fdy * rcp;
              mc_normals[idx].z += fdz * rcp;
              if (wcol) mc_colours[idx] = Color.Lerp(mc_colours[idx], colour, val*2);
            }
            idx++;
          }
        }
      }
    }
    Debug.Log (DateTime.Now - start_time + "s to make values");

    //MarchingCubes(int x0, int y0, int z0, int xdim, int ydim, int zdim, float grid_spacing, float[] mc_values, Vector3[] mc_normals, Color[] mc_colours) {
    MarchingCubes mc = new MarchingCubes(x0, y0, z0, xdim, ydim, zdim, grid_spacing, mc_values, mc_normals, mc_colours);
    start_time = DateTime.Now;

    vertices = mc.vertices;
    normals = mc.normals;
    uvs = mc.uvs;
    colours = mc.colours;
    indices = mc.indices;
  }
  

*/

class molecule_builder {
public:
  molecule_builder(size_t num_atoms, const glslmath::vec3 *pos, const float *radii, const glslmath::vec4 *atom_colours, float resolution) {
    using namespace glslmath;

    printf("molecule_builder(%d, %f)\n", (int)num_atoms, resolution);

    // Create a 3D array for each molecule
    vec3 pmin = pos[0];
    vec3 pmax = pmin;
    float max_r = 0.0f;
    for (size_t j = 0; j != num_atoms; ++j)
    {
      float r = radii[j];
      max_r = std::max(max_r, r);
      pmin = min(pmin, pos[j]);
      pmax = max(pmax, pos[j]);
    }
    
    std::cout << pmin << " " << pmax << " " << max_r << "\n";

    float rgs = resolution;
    int irgs = std::ceil(max_r * rgs) + 1;
    int x0 = std::floor(pmin.x() * rgs) - irgs;
    int y0 = std::floor(pmin.y() * rgs) - irgs;
    int z0 = std::floor(pmin.z() * rgs) - irgs;
    int x1 = std::ceil(pmax.x() * rgs) + irgs;
    int y1 = std::ceil(pmax.y() * rgs) + irgs;
    int z1 = std::ceil(pmax.z() * rgs) + irgs;
    
    std::cout << "irgs=" << irgs << "\n";
    std::cout << "rgs=" << rgs << "\n";

    int xdim = x1-x0+1, ydim = y1-y0+1, zdim = z1-z0+1;
    
    std::cout << vec3(xdim, ydim, zdim) << "\n";

    float threshold = 0.5f;

    values.resize(xdim*ydim*zdim);
    colours.resize(xdim*ydim*zdim);
    std::fill(values.begin(), values.end(), -threshold);

    for (size_t ac = 0; ac != num_atoms; ++ac) {
      vec3 c = pos[ac];
      float r = radii[ac] * (0.8f * rgs);
      //vec4 atom_colour = atom_colours[ac];
      float cx = c.x() * rgs;
      float cy = c.y() * rgs;
      float cz = c.z() * rgs;

      // define a box around the atom.
      int cix = std::floor(c.x() * rgs);
      int ciy = std::floor(c.y() * rgs);
      int ciz = std::floor(c.z() * rgs);
      int xmin = std::max(x0, cix-irgs);
      int ymin = std::max(y0, ciy-irgs);
      int zmin = std::max(z0, ciz-irgs);
      int xmax = std::min(x1, cix+irgs);
      int ymax = std::min(y1, ciy+irgs);
      int zmax = std::min(z1, ciz+irgs);
      float fk = std::log(threshold) / (r * r);
      float fkk = -fk * 0.5f; // 0.5 is a magic number!
      //bool wcol = atom_colour.r != 1.0f || atom_colour.g != 1.0f || atom_colour.b != 1.0f;
      
      for (int z = zmin; z != zmax; ++z) {
        float fdz = z - cz;
        for (int y = ymin; y != ymax; ++y) {
          float fdy = y - cy;
          int idx = ((z-z0) * ydim + (y-y0)) * xdim + (xmin-x0);
          float d2_base = fdy*fdy + fdz*fdz;
          for (int x = xmin; x != xmax; ++x) {
            float fdx = x - cx;
            float d2 = fdx*fdx + d2_base;
            float v = fkk * d2;
            if (v < 1) {
              // a little like exp(-v)
              float val = (2 * v - 3) * v * v + 1;
              values[idx] += val;
              //if (wcol) colours[idx] = mix(colours[idx], atom_colour, val*2);
            }
            idx++;
          }
        }
      }
    }
    
    //for (auto &n : normals) n.normalise();
    
    printf("(%d %d %d) (%d %d %d)\n", x0, y0, z0, xdim, ydim, zdim);
    fflush(stdout);

    marching_cubes mc(x0, y0, z0, xdim, ydim, zdim, 1.0f / resolution, values.data(), colours.data());
    
    sizer s;
    s = mc.get_mesh().write_binary(s);
    image_.resize(s.size());
    std::cout << "size=" << s.size() << "\n";
    std::uint8_t *p = mc.get_mesh().write_binary(image_.data());
    std::cout << "nsize=" << image_.data() - p << "\n";
    if((size_t)(p - image_.data()) != image_.size()) throw(std::range_error("ouch"));
  }
  
  const std::vector<std::uint8_t> &image() const {
    return image_;
  }
private:
  std::vector<std::uint8_t> image_;
  std::vector<float> values;
  std::vector<glslmath::vec4> colours;
};
