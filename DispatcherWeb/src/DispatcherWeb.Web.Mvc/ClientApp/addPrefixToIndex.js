const fs = require('fs');
const path = require('path');

const indexFilePath = path.join(__dirname, 'build', 'index.html');

fs.readFile(indexFilePath, 'utf8', (err, data) => {
    if (err) {
        console.error('Error reading index.html:', err);
        return;
    }

    const modifiedData = data.replace(/(href|src)=("|')(\/(manifest|static|assets)\/)/g, `$1=$2/reactapp$3`);

    fs.writeFile(indexFilePath, modifiedData, 'utf8', (err) => {
        if (err) {
            console.error('Error writing modified index.html:', err);
            return;
        }
        console.log('Modified index.html successfully.');
  
    });
});