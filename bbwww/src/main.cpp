
#define BOOST_ASIO_HAS_MOVE 1
#define BOOST_ASIO_ERROR_CATEGORY_NOEXCEPT noexcept(true)
#define _WIN32_WINNT 0x0501
#define _CRT_SECURE_NO_WARNINGS

#include <boost/asio.hpp>
#include <iostream>
#include <fstream>
#include <vector>
#include <cmath>

#include "json.hpp"

namespace game_server {
  using nlohmann::json;
}

#include "game.hpp"
#include "connection.hpp"
#include "server.hpp"

#include "bioblox.hpp"

int main() {
  boost::asio::io_service io;

  game_server::server svr(io);
  svr.add_game(new game_server::bioblox());
  io.run();
}
