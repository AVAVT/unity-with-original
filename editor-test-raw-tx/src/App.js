import React from 'react';
import { parse } from 'query-string';

const App = () => {
  const [parsedRawTx, setParsedRawTx] = React.useState(null);
  
  React.useEffect(() => {
    const { rawTx } = parse(window.location.search);
    if(rawTx){
      setParsedRawTx(rawTx);
    }
  }, [])
  return (
    <div className="App" style={{minHeight: '100vh', display: 'flex', flexDirection: 'column', justifyContent: 'center', padding: '10px 20px'}}>
      {
        parsedRawTx ? <div style={{textAlign: 'center'}}>
          <h3>Parsed rawTx (paste into corresponding field in Unity Editor):</h3>
          <p style={{wordBreak: 'break-all', wordWrap:'break-word'}}>{parsedRawTx}</p>
          </div>: (<h3 style={{textAlign: 'center'}}>No rawTx params found</h3>)
      }
    </div>
  );
}

export default App;
