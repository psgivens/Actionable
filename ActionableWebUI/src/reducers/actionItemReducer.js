import * as types from '../actions/actionTypes';
import initialState from './initialState';

export default function actionItemReducer(state = initialState.actionItems, action) {
  switch (action.type) {
    case types.LOAD_ACTION_ITEMS_SUCCESS:

      // TODO: set isDirty to false after reloading. 
      return Object.assign(action.actionItems, {isDirty:true});

    case types.CREATE_ACTION_ITEM_SUCCESS:
      return Object.assign([
        ...state,
        Object.assign({}, action.actionItem)
      ], {isDirty:true});

    case types.UPDATE_ACTION_ITEM_SUCCESS:
      return Object.assign( [
        ...state.filter(actionItem => actionItem.id !== action.actionItem.id),
        Object.assign({}, action.actionItem)
      ], {isDirty:true});

    default:
      return state;
  }
}
