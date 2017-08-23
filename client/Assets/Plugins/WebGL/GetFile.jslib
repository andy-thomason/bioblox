var getFile = {
    getFileFromBrowser: function(objectNamePtr, funcNamePtr) {
		
      window.window_getFileFromBrowser =
          window.window_getFileFromBrowser || {
         busy: false,
         initialized: false,
         rootDisplayStyle: null,  // style to make root element visible
         root_: null,             // root element of form
      };
	  
      var g = window.window_getFileFromBrowser;
	  
      if (g.busy) {
          // Don't let multiple requests come in
          return;
      }
	  
      g.busy = true;

      var objectName = Pointer_stringify(objectNamePtr);
      var funcName = Pointer_stringify(funcNamePtr);

      if (!g.initialized) {
          g.initialized = true;

          // Append a form to the page (more self contained than editing the HTML?)
          g.root = window.document.createElement("div");
          g.root.innerHTML = [
            '<style>                                                    ',
            '.getfile {                                                ',
            '    position: absolute;                                    ',
            '    left: 0;                                               ',
            '    top: 0;                                                ',
            '    width: 100%;                                           ',
            '    height: 100%;                                          ',
            '    display: -webkit-flex;                                 ',
            '    display: flex;                                         ',
            '    -webkit-flex-flow: column;                             ',
            '    flex-flow: column;                                     ',
            '    -webkit-justify-content: center;                       ',
            '    -webkit-align-content: center;                         ',
            '    -webkit-align-items: center;                           ',
            '                                                           ',
            '    justify-content: center;                               ',
            '    align-content: center;                                 ',
            '    align-items: center;                                   ',
            '                                                           ',
            '    z-index: 2;                                            ',
            '    color: white;                                          ',
            '    background-color: rgba(0,0,0,0.8);                     ',
            '    font: sans-serif;                                      ',
            '    font-size: x-large;                                    ',
            '}                                                          ',
            '.getfile a,                                               ',
            '.getfile label {                                          ',
            '   font-size: x-large;                                     ',
            '   background-color: #666;                                 ',
            '   border-radius: 0.5em;                                   ',
            '   border: 1px solid black;                                ',
            '   padding: 0.5em;                                         ',
            '   margin: 0.25em;                                         ',
            '   outline: none;                                          ',
            '   display: inline-block;                                  ',
            '}                                                          ',
            '.getfile input {                                          ',
            '    display: none;                                         ',
            '}                                                          ',
            '</style>                                                   ',
            '<div class="getfile">                                     ',
            '    <div>                                                  ',
            '      <label for="file">click to choose a PDB file</label>  ',
            '      <input id="file" type="file" accept=".pdb"/><br/>',
            '      <a>cancel</a>                                        ',
            '    </div>                                                 ',
            '</div>                                                     ',
          ].join('\n');
          var input = g.root.querySelector("input");
          input.addEventListener('change', getPDB);

          // prevent clicking in input or label from canceling
          input.addEventListener('click', preventOtherClicks);
          var label = g.root.querySelector("label");
          label.addEventListener('click', preventOtherClicks);

          // clicking cancel or outside cancels
          var cancel = g.root.querySelector("a");  // there's only one
          cancel.addEventListener('click', handleCancel);
          var getFile = g.root.querySelector(".getfile");
          getFile.addEventListener('click', handleCancel);

          // remember the original style
          g.rootDisplayStyle = g.root.style.display;

          window.document.body.appendChild(g.root);
      }

      // make it visible
      g.root.style.display = g.rootDisplayStyle;

      function preventOtherClicks(evt) {
          evt.stopPropagation();
      }

      function getPDB(evt) {
		evt.stopPropagation();
		var fileInput = evt.target.files;
		if (!fileInput || !fileInput.length) {
		  return sendError("no file selected");
		}
		var PDB = window.URL.createObjectURL(fileInput[0]);
		
		var allText_pdb;
		var rawFile = new XMLHttpRequest();
		rawFile.open("GET", PDB, false);
		rawFile.onreadystatechange = function ()
		{
			if(rawFile.readyState === 4)
			{
				if(rawFile.status === 200 || rawFile.status == 0)
				{
					allText_pdb = rawFile.responseText;
				}
			}
		}
		rawFile.send(null);
		sendResult(allText_pdb);
      }

      function handleCancel(evt) {
          evt.stopPropagation();
          evt.preventDefault();
          sendError();
      }
	  
      function sendError() {
          sendResult("0");
      }

      function hide() {
          g.root.style.display = "none";
      }

      function sendResult(result) {
          hide();
          g.busy = false;
          SendMessage(objectName, funcName, result);
      }
    },
};

mergeInto(LibraryManager.library, getFile);
