const fs = require('fs');
const path = require('path');

const buildDir = './build/static/css/';
const cssFileRegex = /main\.[a-f0-9]{8}\.css/;
const mapFileRegex = /main\.[a-f0-9]{8}\.css\.map/;
const prefix = '/reactapp';

fs.readdir(buildDir, (err, files) => {
    if (err) {
        console.error('Error reading build directory:', err);
        return;
    }

    const cssFileName = files.find((file) => cssFileRegex.test(file));
    const mapFileName = files.find((file) => mapFileRegex.test(file));

    if (!cssFileName) {
        console.error('CSS file not found in build directory.');
        return;
    }
    if (!mapFileName) {
        console.error('Map file not found in build directory.');
        return;
    }

    const cssFilePath = path.join(buildDir, cssFileName);
    const mapFilePath = path.join(buildDir, mapFileName);

    fs.readFile(cssFilePath, 'utf8', (err, data) => {
        if (err) {
            console.error('Error reading CSS file:', err);
            return;
        }

        // Replace each occurrence of /static/media/ with /reactapp/static/media/
        const modifiedCSS = data.replace(/\/static\/media\//g, `${prefix}/static/media/`);

        // Write the modified CSS to a new file
        fs.writeFile(cssFilePath, modifiedCSS, 'utf8', (err) => {
            if (err) {
                console.error('Error writing modified CSS file:', err);
                return;
            }
            console.log('CSS file modified successfully.');
        });
    });

    fs.readFile(mapFilePath, 'utf8', (err, data) => {
        if (err) {
            console.error('Error reading CSS file:', err);
            return;
        }

        // Replace each occurrence of /static/media/ with /reactapp/static/media/
        const modifiedMapFile = data.replace(/\/static\/media\//g, `${prefix}/static/media/`);

        // Write the modified CSS to a new file
        fs.writeFile(mapFilePath, modifiedMapFile, 'utf8', (err) => {
            if (err) {
                console.error('Error writing modified CSS file:', err);
                return;
            }
            console.log('CSS file modified successfully.');
        });
    });
});