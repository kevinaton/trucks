import * as React from 'react';
import {
    Autocomplete,
    Avatar,
    Badge,
    Box,
    CircularProgress,
    Divider,
    FormControl,
    InputAdornment,
    List,
    ListItem,
    ListItemButton,
    ListItemText,
    Paper,
    SwipeableDrawer,
    IconButton,
    TextField,
    Typography,
    Tooltip,
    Button,
    Stack,
    Menu,
    MenuItem,
} from '@mui/material';
import data from '../common/data/data.json';
import { theme } from '../Theme';
import { blue } from '@mui/material/colors';

const { Friends, Chat } = data;
const friendsActive = ['John Doe', 'Rose Lim', 'Micah Median', 'Jeamar Library'];

function sleep(delay = 0) {
    return new Promise((resolve) => {
        setTimeout(resolve, delay);
    });
}

export const ChatPane = ({ state, setState }) => {
    const [selectedFriends, setSelectedFriends] = React.useState([]);
    const [unselectedFriends, setUnselectedFriends] = React.useState([]);
    const [openAutocomplete, setOpenAutocomplete] = React.useState(false);
    const loading = openAutocomplete && unselectedFriends.length === 0;
    const [isChat, setIsChat] = React.useState(false);
    const [chatFriend, setChatFriend] = React.useState({});
    const [friendMenuAnchorEl, setFriendMenuAnchorEl] = React.useState();
    const isFriendSet = Boolean(friendMenuAnchorEl);

    React.useEffect(() => {
        const fetchData = async () => {
            setSelectedFriends(Friends.filter((friend) => friendsActive.includes(friend.name)));
        };

        fetchData();
    }, []);

    React.useEffect(() => {
        let active = true;
        let options = Friends.filter((friend) => !friendsActive.includes(friend.name));

        if (!loading) {
            return undefined;
        }

        (async () => {
            await sleep(1e3); // assuming fetching the data

            if (active) {
                if (options.length === 0) {
                    return () => {
                        active = false;
                    };
                }
                if (unselectedFriends.length === 0) {
                    setUnselectedFriends(options);
                }
            }
        })();

        return () => {
            active = false;
        };
    }, [loading, unselectedFriends, selectedFriends]);

    const toggleDrawer = (open) => (event) => {
        if (event && event.type === 'keydown' && (event.key === 'Tab' || event.key === 'Shift')) {
            return;
        }
        setState(open);
    };

    const handleSelect = (event, value) => {
        const newFriends = Friends.find((friend) => friend.name === value);

        setSelectedFriends((prevFriends) => [...prevFriends, newFriends]);
        setUnselectedFriends((prevFriends) =>
            prevFriends.filter((friend) => friend !== newFriends)
        );
    };

    const handleFriendChat = (friend) => {
        setChatFriend(friend);
        setIsChat(true);
    };

    const handleChatBack = () => {
        setIsChat(false);
        setChatFriend({});
    };

    const handleFriendMenuClick = (event) => {
        setFriendMenuAnchorEl(event.currentTarget);
    };

    const handleFriendMenuClose = () => {
        setFriendMenuAnchorEl(null);
    };

    return (
        <SwipeableDrawer
            anchor='right'
            open={state}
            onClose={toggleDrawer(false)}
            onOpen={toggleDrawer(true)}
            variant='temporary'
            sx={{
                zIndex: 1250,
            }}>
            <Box
                sx={{
                    display: isChat ? 'none' : 'flex',
                    flexDirection: 'column',
                    height: 1,
                    alignContent: 'space-between',
                }}>
                <Box sx={{ width: 400, flexGrow: 1 }}>
                    <Box>
                        <FormControl
                            sx={{
                                width: 1,
                            }}>
                            <Autocomplete
                                value={null}
                                autoComplete={true}
                                onChange={handleSelect}
                                options={unselectedFriends.map((friend) => friend.name)}
                                open={openAutocomplete}
                                onOpen={() => {
                                    setOpenAutocomplete(true);
                                }}
                                onClose={() => {
                                    setOpenAutocomplete(false);
                                }}
                                loading={loading}
                                noOptionsText='No Friends found'
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        placeholder='Search to add friends'
                                        size='small'
                                        value={null}
                                        variant='outlined'
                                        InputProps={{
                                            ...params.InputProps,
                                            startAdornment: (
                                                <InputAdornment position='start'>
                                                    <i className='fa-regular fa-search'></i>
                                                </InputAdornment>
                                            ),
                                            endAdornment: (
                                                <React.Fragment>
                                                    {loading ? (
                                                        <CircularProgress size={14} />
                                                    ) : null}
                                                    <Tooltip
                                                        title={
                                                            <Typography>
                                                                Write only username for some tenant
                                                                users. Ex: tenancyname/username
                                                            </Typography>
                                                        }>
                                                        <IconButton
                                                            sx={{
                                                                width: 16,
                                                                height: 16,
                                                                p: 0,
                                                                m: 0,
                                                                mt: '-3px',
                                                                backgroundColor:
                                                                    theme.palette.grey[150],
                                                            }}>
                                                            <i
                                                                className='fa-regular fa-info'
                                                                style={{
                                                                    fontSize: 8,
                                                                    fontWeight: 'bold',
                                                                }}></i>
                                                        </IconButton>
                                                    </Tooltip>
                                                    {params.InputProps.endAdornment}
                                                </React.Fragment>
                                            ),
                                        }}
                                    />
                                )}
                                PaperComponent={({ children }) => (
                                    <Paper
                                        sx={{ backgroundColor: theme.palette.grey[150] }}
                                        elevation={10}>
                                        {children}
                                    </Paper>
                                )}
                                sx={{
                                    w: 1,
                                    mx: 1,
                                    my: 2,
                                }}
                            />
                        </FormControl>
                        <Typography
                            sx={{ mx: 1 }}
                            variant='subtitle2'
                            color={theme.palette.grey[500]}>
                            FRIENDS
                        </Typography>
                        <Divider />
                    </Box>
                    <List>
                        {selectedFriends.map((friend, index) => {
                            return (
                                <ListItem
                                    disablePadding
                                    key={index}
                                    onClick={() => handleFriendChat(friend)}>
                                    <ListItemButton>
                                        <Badge
                                            color={friend.status}
                                            variant='dot'
                                            overlap='circular'>
                                            <Avatar
                                                alt='account'
                                                src={friend.avatar}
                                                sx={{ mr: 0, width: 24, height: 24 }}
                                            />
                                        </Badge>
                                        <ListItemText primary={friend.name} sx={{ ml: 1 }} />
                                    </ListItemButton>
                                </ListItem>
                            );
                        })}
                    </List>
                </Box>
                <Box sx={{ width: 400 }}>
                    <Box>
                        <Typography
                            sx={{ mx: 1, mt: 1 }}
                            variant='subtitle2'
                            color={theme.palette.grey[500]}>
                            BLOCKED USERS
                        </Typography>
                        <Divider />
                    </Box>
                    <List>
                        <Typography sx={{ m: 1 }} color={theme.palette.grey[400]}>
                            You don't have any blocked users. In order to block a friend, select a
                            friend and select block from actions dropdown.
                        </Typography>
                    </List>
                </Box>
            </Box>
            <Box
                sx={{
                    display: isChat ? 'flex' : 'none',
                    width: 400,
                    flexDirection: 'column',
                    height: 1,
                    alignContent: 'space-between',
                }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 2, px: 1 }}>
                    <Box sx={{ display: 'flex', flexGrow: 1 }}>
                        <IconButton onClick={handleChatBack}>
                            <i className='fa-regular fa-chevron-left'></i>
                        </IconButton>
                        <Badge
                            color={chatFriend.status}
                            variant='dot'
                            overlap='circular'
                            sx={{ mr: 1 }}>
                            <Avatar
                                alt='account'
                                src={chatFriend.avatar}
                                sx={{ mr: 0, width: 24, height: 24 }}
                            />
                        </Badge>
                        <Typography sx={{ fontWeight: 600 }}>{chatFriend.name}</Typography>
                    </Box>
                    <IconButton
                        id='friend-menu-button'
                        aria-controls={isFriendSet ? 'friend-menu' : undefined}
                        aria-haspopup='true'
                        aria-expanded={isFriendSet ? 'true' : undefined}
                        onClick={handleFriendMenuClick}>
                        <i className='fa-regular fa-ellipsis-vertical'></i>
                    </IconButton>
                    <Menu
                        id='friend-menu'
                        anchorEl={friendMenuAnchorEl}
                        open={isFriendSet}
                        onClose={handleFriendMenuClose}
                        MenuListProps={{
                            'aria-labelledby': 'friend-menu-button',
                        }}>
                        <MenuItem onClick={handleFriendMenuClose}>
                            <i
                                className='fa-regular fa-ban secondary-icon'
                                style={{ marginRight: 4 }}></i>
                            Block
                        </MenuItem>
                    </Menu>
                    <Divider />
                </Box>
                <Box
                    id='chat-container'
                    sx={{
                        flexGrow: 1,
                        backgroundColor: theme.palette.grey[200],
                        overflowY: 'auto',
                        maxHeight: 'calc(100vh - 80px)',
                    }}>
                    <Stack direction='column' spacing={2} sx={{ p: 2 }}>
                        {Chat.map((chat, index) => {
                            return (
                                <Paper
                                    key={index}
                                    sx={{
                                        backgroundColor:
                                            chat.source === 'friend' ? blue[700] : blue[100],
                                        py: 1,
                                        px: 2,
                                        borderRadius: 3,
                                        width: '70%',
                                        alignSelf: chat.source === 'friend' ? 'none' : 'flex-end',
                                    }}>
                                    <Typography
                                        color={
                                            chat.source === 'friend'
                                                ? '#fff'
                                                : theme.palette.text.primary
                                        }>
                                        {chat.content}
                                    </Typography>
                                </Paper>
                            );
                        })}
                    </Stack>
                </Box>
                <Box
                    id='chat-box'
                    sx={{
                        py: 2,
                        px: 1,
                        display: 'flex',
                        alignContent: 'center',
                        position: 'sticky',
                        bottom: 0,
                        zIndex: 3,
                    }}>
                    <Divider />
                    <TextField
                        variant='outlined'
                        size='small'
                        placeholder='Type here'
                        sx={{ flexGrow: 1, mr: 1, backgroundColor: '#fff' }}
                    />
                    <Button
                        variant='contained'
                        endIcon={
                            <i className='fa-regular fa-paper-plane' style={{ fontSize: 12 }}></i>
                        }>
                        Send
                    </Button>
                </Box>
            </Box>
        </SwipeableDrawer>
    );
};

export default ChatPane;
