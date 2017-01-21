import * as types from './actionTypes';
import ActionItemApi from '../api/ActionItemApi';
import { takeEvery, delay } from 'redux-saga';
import { call, put, take, select } from 'redux-saga/effects';
import { push, routeActions } from 'react-router-redux';

//import {beginAjaxCall, ajaxCallError} from './ajaxStatusActions';

export function loadActionItemSuccess(actionItems) {
  return { type: types.LOAD_ACTION_ITEMS_SUCCESS, actionItems};}
export function createActionItemSuccess(actionItem) {
  return {type: types.CREATE_ACTION_ITEM_SUCCESS, actionItem};}
export function updateActionItemSuccess(actionItem) {
  return {type: types.UPDATE_ACTION_ITEM_SUCCESS, actionItem};}
export function requestLoadActionItems () {
  return {type: types.LOAD_ACTION_ITEMS_REQUEST};}
export function requestSaveActionItem(actionItem) {
  return {type: types.CREATE_ACTION_ITEM_REQUEST, actionItem};}
export function requestDeleteActionItem(actionItemId) {
  return {type: types.DELETE_ACTION_ITEM_REQUEST, actionItemId};}
export function deleteActionItemSuccess () {
  return {type: types.DELETE_ACTION_ITEM_SUCCESS};}
export function requestUpdateActionItem (actionItem) {
  return {type: types.UPDATE_ACTION_ITEM_REQUEST, actionItem};}

const getAuthentication = state => state.authentication;

function* handleCreateActionItemRequest(action) {
  const authentication = yield select(getAuthentication);
  const item = Object.assign({}, action.actionItem, {Fields: {key:"value"}});
  yield call(ActionItemApi.saveActionItem, authentication, action.actionItem);
  yield put(createActionItemSuccess(action.actionItem));
  yield put(requestLoadActionItems());
  yield put(push('/actions'));}

function* handleUpdateActionItemRequest(action) {
  const authentication = yield select(getAuthentication);
  yield call(ActionItemApi.updateActionItem, authentication, action.actionItem);
  yield put(updateActionItemSuccess(action.actionItem));
  yield put(requestLoadActionItems());
  yield put(push('/actions'));}

function* handleLoadActionItemsRequest (action) {
  const authentication = yield select(getAuthentication);
  try {
    const webResult = yield call(ActionItemApi.getAllActionItems, authentication);
    yield put(loadActionItemSuccess(webResult.results));
  } catch (error) {
  }}

function* handleDeleteActionItemRequest(action) {
  const authentication = yield select(getAuthentication);
  try {
    yield call(ActionItemApi.deleteActionItem, authentication, action.actionItemId);
    yield put(deleteActionItemSuccess());
    yield put(requestLoadActionItems());
    yield put(push('/actions'));
  } catch (error) {

  }}

export function* watchForActionItemsRequests () {
  yield takeEvery(types.LOAD_ACTION_ITEMS_REQUEST, handleLoadActionItemsRequest);
  yield takeEvery(types.CREATE_ACTION_ITEM_REQUEST, handleCreateActionItemRequest);
  yield takeEvery(types.DELETE_ACTION_ITEM_REQUEST, handleDeleteActionItemRequest);
  yield takeEvery(types.UPDATE_ACTION_ITEM_REQUEST, handleUpdateActionItemRequest);
  }
