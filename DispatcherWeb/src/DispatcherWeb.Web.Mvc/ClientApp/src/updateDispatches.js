const fs = require("fs");

const data = fs.readFileSync("./src/data.json", "utf8");
const jsonData = JSON.parse(data);

const currentDate = new Date().toISOString().slice(0, 10); //Get current date

jsonData.dispatches.forEach((dispatch) => {
  const startTime = dispatch.start.slice(11); //Get the sart time
  const endTime = dispatch.end.slice(11); //Get the end time

  dispatch.start = `${currentDate}T${startTime}`; //updated the start date of the dispatch
  dispatch.end = `${currentDate}T${endTime}`; //updated the end date of the dispatch
});

fs.writeFileSync("./src/data.json", JSON.stringify(jsonData));
