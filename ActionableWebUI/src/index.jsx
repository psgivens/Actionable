import './main.css';
//
// import React from 'react';
// import ReactDOM from 'react-dom';
// import App from './components/App.jsx';
//
//
// ReactDOM.render(<div><App /></div>, document.getElementById('app'));

global.jQuery = require('jquery');
require('bootstrap-loader');
import React from 'react';
import { render } from 'react-dom';
import configureStore from './store/configureStore';
import {requestLoadActionItems} from './actions/actionItemActions';
import {Provider} from 'react-redux';
// import Routes from './routes';
import {syncHistoryWithStore} from 'react-router-redux';

import {Router, Route, hashHistory, IndexRoute} from 'react-router';

import LoginPage from './components/authentication/LoginPage';
import LogoutPage from './components/authentication/LogoutPage';

import App from './components/App';
import HomePage from './components/home/HomePage';
import ActionItemsPage from './components/actionItems/ActionItemsPage';
import EditActionItemPage from './components/actionItems/EditActionItemPage';
import StatusPage from './components/status/StatusPage';

//import {push, routeActions} from 'react-router-redux';

const store = configureStore();
store.dispatch(requestLoadActionItems());
const history = syncHistoryWithStore(hashHistory, store);

// Thank you to this post
// https://github.com/ReactTraining/react-router/blob/master/examples/auth-flow/app.js#L117
// also mentioned in this post
// https://github.com/ReactTraining/react-router/issues/1388
function requireAuth(nextState, replace) {
  const state = store.getState();
  if (!state.authentication.isAuthenticated) {
    replace({
      pathname: '/login',
      // See blog post for use of location in the login redirect
      state: { nextPathname: nextState.location.pathname }
    })
  }
}

// function stopAndDebug (nextState, replace) {
//   debugger;
// }

render((
  <Provider store={store} >
    <Router history={history}>
        <Route path="/" component={App} >
            <IndexRoute component={StatusPage} />
            <Route path="/login" component={LoginPage} />
            <Route path="/logout" component={LogoutPage} />
            <Route path="/home" component={HomePage} />
            <Route path="/actions" component={ActionItemsPage} onEnter={requireAuth} />
            <Route path="/editactionitem" component={EditActionItemPage} onEnter={requireAuth} />
            <Route path="/editactionitem/:id" component={EditActionItemPage} onEnter={requireAuth} />
            <Route path="/status" component={StatusPage} />
        </Route>
      </Router>
  </Provider>
), document.getElementById('app'));
