
namespace game_server {

  // generic game class.
  class bioblox : public game {
  public:
    bioblox() {
    }

    void update(const json &input, const json &output) {
      std::cout << input.dump();
    }
  private:
  };
}
