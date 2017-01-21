import {combineReducers} from 'redux';
import actionItems from './actionItemReducer';
import authentication from './authenticationReducer';
// import ajaxCallsInProgress from './ajaxStatusReducer';

import {routerReducer} from 'react-router-redux';


const rootReducer = combineReducers({
  actionItems,
  authentication,
  // ajaxCallsInProgress
  routing: routerReducer
});

export default rootReducer;
