mergeInto(LibraryManager.library, {

  download_file: function (str) {
    window.open('http://13.58.210.151/download_file.php?file_name='+Pointer_stringify(str),'_blank');
  },


});
