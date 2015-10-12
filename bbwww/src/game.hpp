#include <cmath>
#include <unordered_map>

namespace game_server {

  // generic game class.
  class game {
  public:
    game() {
    }

    virtual void update(const json &input, const json &output) {
    }

    // simulate the game
    void do_frame(const std::string &data_in, std::string &data_out) {
      json input = json::parse(data_in.c_str());
      json output;

      update(input, output);
      frame_number_++;

      data_out = output.dump();
    }

  private:

    int frame_number_ = 0;
  };
}
