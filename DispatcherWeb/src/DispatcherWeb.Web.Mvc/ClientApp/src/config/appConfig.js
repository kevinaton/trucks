let App = {};

App.Enums = App.Enums || {};

// dispatch status
App.Enums.DispatchStatus = {
    created: 0,
    sent: 1,
    acknowledged: 3,
    loaded: 4,
    completed: 5,
    error: 6,
    canceled: 7
};

// designation
App.Enums.Designation = {
    freightOnly: 1,
    materialOnly: 2,
    freightAndMaterial: 3,
    //rental: 4,
    backhaulFreightOnly: 5,
    backhaulFreightAndMaterial: 9,
    disposal: 6,
    backHaulFreightAndDisposal: 7,
    straightHaulFreightAndDisposal: 8
};

// designations
App.Enums.Designations = {
    hasMaterial: [
        App.Enums.Designation.materialOnly,
        App.Enums.Designation.freightAndMaterial,
        App.Enums.Designation.backhaulFreightAndMaterial,
        App.Enums.Designation.disposal,
        App.Enums.Designation.backHaulFreightAndDisposal,
        App.Enums.Designation.straightHaulFreightAndDisposal
    ],
    materialOnly: [
        App.Enums.Designation.materialOnly
    ],
    freightOnly: [
        App.Enums.Designation.freightOnly,
        App.Enums.Designation.backhaulFreightOnly
    ],
    freightAndMaterial: [
        App.Enums.Designation.freightAndMaterial,
        App.Enums.Designation.backhaulFreightAndMaterial,
        App.Enums.Designation.disposal,
        App.Enums.Designation.backHaulFreightAndDisposal,
        App.Enums.Designation.straightHaulFreightAndDisposal
    ]
};

export default App;