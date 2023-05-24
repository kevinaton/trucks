import React, { useEffect } from "react";
import {
  Box,
  Button,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Typography,
} from "@mui/material";
import { Helmet, HelmetProvider } from "react-helmet-async";
import NoContent from "../../components/NoContent";
import data from "../../common/data/data.json";
import { DataGrid } from "@mui/x-data-grid";
import { NotificationSettings } from "../../components/DTComponents";

let { Notifications } = data;
let action = "none";

const ReadMenu = (params) => {
  return (
    <div>
      <IconButton onClick={() => (action = "read")}>
        {params.row.isRead === true ? (
          <i className="fa-regular fa-eye-slash secondary-icon"></i>
        ) : (
          <i className="fa-regular fa-eye"></i>
        )}
      </IconButton>
      <IconButton onClick={() => (action = "delete")}>
        <i className="fa-regular fa-close secondary-icon"></i>
      </IconButton>
    </div>
  );
};

const tableColumns = [
  {
    field: "content",
    headerName: "Notifications",
    flex: 1,
    minWidth: 200,
    headerClassName: "tableHeader",
  },
  {
    field: "time",
    headerName: "Date",
    flex: 0.15,
    minWidth: 100,
    headerClassName: "tableHeader",
  },
  {
    field: "action",
    headerName: "",
    flex: 0.08,
    minWidth: 40,
    renderCell: ReadMenu,
    headerClassName: "tableHeader",
  },
];

const NotificationsPage = (props) => {
  const pageName = "Notifications";
  const [filter, setFilter] = React.useState("all");
  const [dataNotifications, setDataNotifications] =
    React.useState(Notifications);
  const [viewNotifSet, setViewNotifSet] = React.useState(false);

  useEffect(() => {
    props.handleCurrentPageName(pageName);
  }, [props]);

  const handleActionMenu = async (params) => {
    if (params.field === "action") {
      const result = await action;
      if (result === "read") {
        const updatedNotif = dataNotifications.map((item) => {
          if (params.row.id === item.id) {
            return {
              ...item,
              isRead: !item.isRead,
            };
          }
          return item;
        });
        setDataNotifications(updatedNotif);
      }
      if (result === "delete") {
        const updatedNotif = dataNotifications.filter(
          (notif) => notif.id !== params.row.id
        );
        setDataNotifications(updatedNotif);
      }
    }
  };

  const handleFilter = (event) => {
    setFilter(event.target.value);
  };

  // Handles settings all notifications to read
  const handleReadAll = () => {
    const updatedNotif = dataNotifications.map((item) => {
      if (item.isRead === false) {
        return {
          ...item,
          isRead: true,
        };
      }
      return item;
    });
    setDataNotifications(updatedNotif);
  };

  return (
    <HelmetProvider>
      <div>
        <Helmet>
          <meta charSet="utf-8" />
          <title>{pageName}</title>
          <meta name="description" content="Dumptruckdispatcher app" />
          <meta content="" name="author" />
          <meta property="og:title" content={pageName} />
          <meta
            property="og:image"
            content="%PUBLIC_URL%/assets/dumptruckdispatcher-logo-mini.png"
          />
        </Helmet>
        <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
          <Typography variant="h6" component="h2" sx={{ mb: 1 }}>
            {pageName}
          </Typography>
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" onClick={() => setViewNotifSet(true)}>
              Notification Settings
            </Button>
            <NotificationSettings
              state={viewNotifSet}
              setViewNotifSet={setViewNotifSet}
            />
            <Button variant="outlined" onClick={handleReadAll}>
              Set all as read
            </Button>
          </Stack>
        </Box>
        <Paper sx={{ p: 2 }}>
          <Box>
            <FormControl sx={{ minWidth: 120, mb: 2 }} size="small">
              <InputLabel>Filter</InputLabel>
              <Select
                id="notification-filter"
                value={filter}
                label="Filter"
                onChange={handleFilter}
              >
                <MenuItem value="all">All</MenuItem>
                <MenuItem value="unread">Unread</MenuItem>
                <MenuItem value="read">Read</MenuItem>
              </Select>
            </FormControl>
          </Box>
          <div style={{ height: 600, width: "100%" }}>
            <DataGrid
              rows={dataNotifications}
              columns={tableColumns}
              onCellClick={(params) => {
                handleActionMenu(params);
              }}
              initialState={{
                pagination: {
                  paginationModel: { page: 0, pageSize: 10 },
                },
              }}
              pageSizeOptions={[10, 30, 50, 100]}
              autoHeight
            />
          </div>
        </Paper>
      </div>
    </HelmetProvider>
  );
};

export default NotificationsPage;
