function modifyUnityInstanceForHaste() {
  const canvas = document.querySelector("canvas"),
    canvasDims = { width: 960, height: 540 };

  // keep content dimensions proportionate
  canvas.width = canvasDims.width;
  canvas.height = canvasDims.height;
  // Edit the dimensions to see how the sizing adapts to different aspect ratios. This will be used in later calculations to determine the proper fit. If you're on a larger device, try resizing the screen to see how the canvas adapts and maintains aspect ratio.

  function theatorFit() {
    /*
    calculate dimensions for if the width was the screen width, and the height was still proportionate. This will be used in a later calculation.
    
    formula for getting new height using new width and aspect ratio:
    aspectRatio = oldHeight/oldWidth;
    newHeight = aspectRatio*newWidth;
    */
    let dims = {
      width: window.innerWidth,
      height: (canvasDims.height / canvasDims.width) * window.innerWidth
    };

    /* set aspect ratio in CSS. This ensures that the element is always a certain aspect ratio, even if width or height isn't given. 
    
    aspect ratio is given by the formula: width/height = aspectRatio;
    */
    canvas.style.aspectRatio = canvasDims.width / canvasDims.height;

    /*
    use "dims" to determine proper fit.
  
    If the screen height is more than the new canvas height when the width of the canvas is maximized, that means theres vertical space, and none of the height will be cut off, so we maximize the width, and set height to initial as to allow it to naturally conform to the aspect ratio (incase it was previously set to 100%).
  
  Otherwise, if the new canvas height is more than the screen height, that means some of the canvas height will be cut off by the viewport, therefore we should maximize the height instead, and set the width to initial.
    */
    if (window.innerHeight > dims.height) {
      canvas.style.width = "100%";
      canvas.style.height = "initial";
      canvas.style.top = `calc(50% - ${dims.height + "px"}/2)`;
    } else {
      canvas.style.height = "100%";
      canvas.style.width = "initial";
      canvas.style.top = "initial";
    }
  }

  window.onresize = theatorFit;

  theatorFit();
}



function hasteLogin() {
  window.hasteClient.login();
}

function iphoneSoundFix() {
  const context = new (window.AudioContext || window.webkitAudioContext)();
unmute(context, true, false);

const buffer = context.createBuffer(1, 1, 22050); // 1/10th of a second of silence
const source = context.createBufferSource();
source.buffer = buffer;
source.connect(context.destination);
let hasStarted = false;


const play = () => {
  if (source && !hasStarted) {
    hasStarted = true;
    source.start();
  }
};
window.alert = console.log;
window.addEventListener('mousedown', play);
}

function createUnityInstance() {
      var canvas = document.querySelector("#unity-canvas");
      var isLoaded = false;
      var buildUrl = "Build";
      var loaderUrl = buildUrl + "/WebGL.loader.js";
      var script = document.createElement("script");

      script.src = loaderUrl;

      script.onload = () => {
        createUnityInstance(document.querySelector("#unity-canvas"), {
        dataUrl: "Build/WebGL.data",
        frameworkUrl: "Build/WebGL.framework.js",
        codeUrl: "Build/WebGL.wasm",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "api.sample.hasteoriginals.com",
        productName: "ExampleUnityGameHasteArcade",
        productVersion: "1",
      }).then((unityInstance) => {
          document.querySelector(".loading-container").style.display = 'none';
          document.querySelector("#unity-container-parent").style.display = 'block';
          canvas.style.border = '1px solid rgba(255, 255, 255, 0.25)';
          canvas.style.position = 'relative';
          isLoaded = true;
          modifyUnityInstanceForHaste();
        }).catch((message) => {
          alert(message);
        });

      };

    document.body.appendChild(script);
}

async function hasteInst() {
    try {
        window.hasteClient = await haste.HasteClient.build();
        const token = await window.hasteClient.getTokenDetails();
        if(!token || !token.isAuthenticated) {
          document.querySelector(".loading-container").style.display = 'none';
          document.querySelector("#login-container").style.display = 'block';
        } else {
          window.token = token;
          createUnityInstance();
        }
    } catch (e) {
        console.log(e)
    }
}

async function logoutHaste() {
  window.hasteClient.logout();
}
