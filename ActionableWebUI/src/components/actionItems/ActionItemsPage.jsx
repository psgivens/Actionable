import React, {PropTypes} from 'react';
import {connect} from 'react-redux';
import {bindActionCreators} from 'redux';
import {hashHistory} from 'react-router';
//import {push} from 'react-router-redux';

import ActionItemsList from './ActionItemsList';
import Button from '../../common/Button';
import * as actionItemActions from '../../actions/actionItemActions';
import * as authenticationActions from '../../actions/authenticationActions';

const LoadState = {
  NotLoaded: 0,
  Loading: 1,
  Loaded: 2
}

class ActionItemsPage extends React.Component {
  constructor (props, context) {
    super (props, context);
    this.state = {
      loadState: LoadState.NotLoaded
    };
    this.deleteActionItem = this.deleteActionItem.bind(this);
    this.markItemAsDone = this.markItemAsDone.bind(this);
  }

  componentWillMount (nextProps, nextState){

    // TODO: Check authentication
    //this.props.actions.requestDeleteActionItem(actionItemId);
    if (!this.state.loadState === LoadState.NotLoaded || this.props.actionItems.isDirty){
      this.props.actions.requestLoadActionItems ();
      this.setState({loadState: LoadState.Loading});
    }

    // return a boolean value
    return true;
  }

  componentWillUnmount () {
    this.setState({loadState: LoadState.NotLoaded});
  }

  onAddItemClick (event) {
    event.preventDefault ();
    hashHistory.push("/editactionitem");
    //this.context.router.push("/editactionitem");
  }

  deleteActionItem (actionItemId) {
    this.props.actions.requestDeleteActionItem(actionItemId);
  }

  markItemAsDone (actionItem) {
    const item = Object.assign({}, actionItem, {fields:{"actionable.status":10}});
    this.props.actions.requestUpdateActionItem(item);
  }

  render () {
    const {actionItems} = this.props;
    return (
      <div>
        <ActionItemsList
          actionItems={actionItems}
          deleteActionItem={this.deleteActionItem}
          updateActionItem={this.markItemAsDone} />
        <Button text="Add Item" onClick={this.onAddItemClick} />
      </div>);
  }
}

ActionItemsPage.propTypes = {
  actionItems: PropTypes.array.isRequired
};
ActionItemsPage.contextTypes = {
  router: PropTypes.object
};

function mapDispatchToProps(dispatch) {
  return {
    actions: bindActionCreators(actionItemActions, dispatch),
    authActions: bindActionCreators(authenticationActions, dispatch)
  };
}
function mapStateToProps(state, ownProps) {
  return {
    actionItems: state.actionItems,
    isActionItemsDirty: state.isActionItemsDirty,
    isAuthenticated: state.authentication.isAuthenticated
  };
}

export default connect (mapStateToProps, mapDispatchToProps) (ActionItemsPage);
