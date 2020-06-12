chrome.runtime.onInstalled.addListener(function() {
    // Replace all rules ...
    if (!chrome.declarativeContent.onPageChanged)
        chrome.declarativeContent.onPageChanged.addRules([
            {
            // That fires when a page's URL contains a 'g' ...
            conditions: [
                i => true
            ],
            // And shows the extension's page action.
            actions: [ () => {
                const zoom = () => {
                    $("canvas").style.height = $("canvas").style.height.substr(0, 3) * 4 + "px";
                    $("canvas").style.width = $("canvas").style.width.substr(0, 3) * 4 + "px";
                }
                zoom();
            } ]
            }
        ]);
    chrome.declarativeContent.onPageChanged.removeRules(undefined, function() {
      // With a new rule ...
      chrome.declarativeContent.onPageChanged.addRules([
        {
          // That fires when a page's URL contains a 'g' ...
          conditions: [
            i => true
          ],
          // And shows the extension's page action.
          actions: [ () => {
            const zoom = () => {
                $("canvas").style.height = $("canvas").style.height.substr(0, 3) * 4 + "px";
                $("canvas").style.width = $("canvas").style.width.substr(0, 3) * 4 + "px";
            }
            zoom();
          } ]
        }
      ]);
    });
  });

