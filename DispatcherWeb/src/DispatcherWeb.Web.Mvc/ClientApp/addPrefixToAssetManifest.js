const fs = require('fs');
const path = require('path');

const assetManifestPath = path.join(__dirname, 'build', 'asset-manifest.json');
const prefix = '/reactapp';

fs.readFile(assetManifestPath, 'utf8', (err, data) => {
    if (err) {
        console.error('Error reading asset-manifest.json:', err);
        return;
    }

    const manifest = JSON.parse(data);
    const updatedManifest = { ...manifest };

    const updateValueWithPrefix = (value) => {
        if (value.startsWith('/static')) {
            return prefix + value;
        }
        return value;
    };
  
    updatedManifest.files = Object.entries(manifest.files).reduce((result, [key, value]) => {
        result[key] = updateValueWithPrefix(value);
        return result;
    }, {});
  
    updatedManifest.entrypoints = manifest.entrypoints.map((value) => updateValueWithPrefix(value));
  
    fs.writeFile(assetManifestPath, JSON.stringify(updatedManifest, null, 2), 'utf8', (err) => {
        if (err) {
            console.error('Error writing modified asset-manifest.json:', err);
            return;
        }
  
        console.log('Modified asset-manifest.json successfully.');
    });
});