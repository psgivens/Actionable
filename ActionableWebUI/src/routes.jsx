import React from 'react';
// import {Router, Route, hashHistory, IndexRoute} from 'react-router';
//
// import App from './components/App';
// import HomePage from './components/home/HomePage';
// import ActionItemsPage from './components/actionItems/ActionItemsPage';
// import EditActionItemPage from './components/actionItems/EditActionItemPage';

export default class Routes extends React.Component {
  render () {
      return (<Router history={hashHistory}>
          <Route path="/" component={App} >
              <IndexRoute component={HomePage} />
              <Route Path="/login" component={LoginPage} />
              <Route path="/home" component={HomePage} />
              <Route path="/editactionitem" component={EditActionItemPage} />
              <Route path="/editactionitem/:id" component={EditActionItemPage} />
          </Route>
        </Router>);
  }
}
