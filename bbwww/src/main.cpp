
#define BOOST_ASIO_HAS_MOVE 1
#define BOOST_ASIO_ERROR_CATEGORY_NOEXCEPT noexcept(true)
#define _WIN32_WINNT 0x0501
#define _CRT_SECURE_NO_WARNINGS

#include <boost/asio.hpp>
#include <iostream>
#include <fstream>
#include <vector>
#include <cmath>

#include <Windows.h>

#include "json.hpp"

namespace game_server {
  using nlohmann::json;
}

#include "game.hpp"
#include "connection.hpp"
#include "server.hpp"

#include "bioblox.hpp"

int main() {
  /*STARTUPINFO si;
  PROCESS_INFORMATION pi;
  si.cb = sizeof(si);
  memset(&si, 0, sizeof(si));
  memset(&pi, 0, sizeof(pi));
  ::CreateProcessA("c:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe", "http://localhost:8888", nullptr, nullptr, FALSE, NORMAL_PRIORITY_CLASS, NULL, NULL, &si, &pi);
  */

  boost::asio::io_service io;
  game_server::server svr(io);
  svr.add_game(new game_server::bioblox());
  io.run();
}
