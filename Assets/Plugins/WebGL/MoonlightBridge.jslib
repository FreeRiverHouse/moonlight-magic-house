mergeInto(LibraryManager.library, {
  PostMessageToParent: function(jsonPtr) {
    var json = UTF8ToString(jsonPtr);
    try {
      var data = JSON.parse(json);
      window.parent.postMessage(data, '*');
    } catch(e) {
      console.warn('[MoonlightBridge] postMessage failed:', e);
    }
  }
});
