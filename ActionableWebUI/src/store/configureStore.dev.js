import {createStore, combineReducers, applyMiddleware} from 'redux';
import rootReducer from '../reducers/rootReducer';
import reduxImmutableStateInvariant from 'redux-immutable-state-invariant';
//import thunk from 'redux-thunk';
import createSagaMiddleware from 'redux-saga';

//import { helloSaga } from '../sagas/helloSaga';
import {watchForActionItemsRequests} from '../actions/actionItemActions';
import {watchForAuthenticationRequests} from '../actions/authenticationActions';
import {watchForRouterRequests} from '../actions/routerActions';
import {routerMiddleware} from 'react-router-redux';
import {hashHistory} from 'react-router';

const sagaMiddleware = createSagaMiddleware();
export default function configureStore(initialState) {
  const store = createStore(
    rootReducer,
    initialState,
    applyMiddleware(sagaMiddleware,routerMiddleware(hashHistory))
  );
  sagaMiddleware.run(watchForActionItemsRequests);
  sagaMiddleware.run(watchForAuthenticationRequests);
  sagaMiddleware.run(watchForRouterRequests);
  return store;
}
