import React from "react";
import { parse } from "query-string";

const App = () => {
  const [parsedRawTx, setParsedRawTx] = React.useState(null);
  const [copied, setCopied] = React.useState(false);

  React.useEffect(() => {
    const { rawTx } = parse(window.location.search);
    if (rawTx) {
      setParsedRawTx(rawTx);
    }
  }, []);

  function fallbackCopyTextToClipboard(text) {
    const textArea = document.createElement("textarea");
    textArea.value = text;

    // Avoid scrolling to bottom
    textArea.style.top = "0";
    textArea.style.left = "0";
    textArea.style.position = "fixed";

    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
      const successful = document.execCommand("copy");
      return successful;
    } catch (err) {
      console.error("Unable to copy", err);
    }

    document.body.removeChild(textArea);
  }

  const copyTextToClipboard = async (text) => {
    if (!navigator.clipboard) {
      return fallbackCopyTextToClipboard(text);
    }

    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch (err) {
      console.error("Could not copy text: ", err);
      return false;
    }
  };

  const showCopied = React.useCallback(() => {
    setCopied(true);
    setTimeout(() => setCopied(false), 500);
  }, []);

  return (
    <div
      className="App"
      style={{
        minHeight: "100vh",
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        padding: "10px 20px",
      }}
    >
      {parsedRawTx ? (
        <div style={{ textAlign: "center" }}>
          <h3>Success!</h3>
          <p>
            Paste the following code when instructed in game to complete your
            registration:
          </p>
          <div style={{ textAlign: "left", width: "50vw", margin: "0 auto" }}>
            <div>
              <button
                type="button"
                onClick={() =>
                  copyTextToClipboard(parsedRawTx).then(() => showCopied())
                }
              >
                {copied ? "Code Copied!" : "Copy to Clipboard"}
              </button>
            </div>
            <textarea
              readOnly
              value={parsedRawTx}
              rows="5"
              style={{ width: "100%" }}
            />
          </div>
        </div>
      ) : (
        <h3 style={{ textAlign: "center" }}>Error: No code found</h3>
      )}
    </div>
  );
};

export default App;
