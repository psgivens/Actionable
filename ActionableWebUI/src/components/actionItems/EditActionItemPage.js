import React, {PropTypes} from 'react';
import {connect} from 'react-redux';
import {bindActionCreators} from 'redux';
import * as actionItemActions from '../../actions/actionItemActions';
import ActionItemForm from './ActionItemForm';
import toastr from 'toastr';

export class EditActionItemPage extends React.Component {
  constructor(props, context) {
    super(props, context);

    this.state = {
      actionItem: Object.assign({id:'', fields: {
        'actionable.title':'one',
        'actionable.description':'desc',
        'actionable.status':'2'}}, props.actionItem),
      isNewItem: props.isNewItem,
      errors: {},
      saving: false
    };


    this.updateActionItemField = this.updateActionItemField.bind(this);
    this.saveActionItem = this.saveActionItem.bind(this);
  }


  componentWillReceiveProps(nextProps) {
    if (this.props.actionItem.id != nextProps.actionItem.id) {
      // Necessary to populate form when existing course is loaded directly.
      this.setState({actionItem: Object.assign({}, nextProps.actionItem)});
    }
  }

  // componentWillReceiveProps(nextProps) {
  //   if (this.props.actionItem.id != nextProps.actionItem.id) {
  //     // Necessary to populate form when existing course is loaded directly.
  //     this.setState({actionItem: Object.assign({}, nextProps.actionItem)});
  //   }
  // }

  updateActionItemField(event) {
    event.preventDefault();
    const field = event.target.name;
    let actionItem = this.state.actionItem;
    actionItem.fields[field] = event.target.value;
    return this.setState({actionItem: actionItem});
  }

  actionItemFormIsValid () {
    let formIsValid = true;
    let errors = {};

    // if (this.state.actionItem.description.length < 5) {
    //   errors.title = 'Description must be at least 5 characters.';
    //   formIsValid = false;
    // }

    this.setState({errors: errors});
    return formIsValid;
  }

  saveActionItem(event) {
    event.preventDefault();

    if (!this.actionItemFormIsValid ()) {
      return;
    }

    this.setState({saving: true});

    if (this.state.isNewItem) {
      this.props.actions.requestSaveActionItem(this.state.actionItem);
    } else {
      this.props.actions.requestUpdateActionItem(this.state.actionItem);
    }
//      .then(() => this.redirect());
      // .then(() => this.redirect())
      // .catch(error => {
      //   toastr.error(error);
      //   this.setState({saving: false});
      // });

    this.setState({saving: false});
  }


  redirect () {
    this.setState({saving: false});
    toastr.success('Action Item saved');
    this.context.router.push('/actionitems');
  }

  render () {
    return (
      <ActionItemForm
        onChange={this.updateActionItemField}
        onSave={this.saveActionItem}
        actionItem={this.state.actionItem}
        errors={this.state.errors}
        saving={this.state.saving}
      />
    );
  }
}

EditActionItemPage.propTypes = {
  actionItem: PropTypes.object,
  actions: PropTypes.object.isRequired,
  isNewItem: PropTypes.bool.isRequired
};

//Pull in the React Router context so router is available on this.context.router.
EditActionItemPage.contextTypes = {
  router: PropTypes.object
};

function getActionItemById(actionItems, id) {
  const actionItem = actionItems.filter(actionItem => actionItem.id == id);
  return actionItem ? actionItem[0] : null;
}

function mapStateToProps(state, ownProps) {
  const actionItemId = ownProps.params.id; // from the path `/course/:id`

  const actionItem = getActionItemById(state.actionItems, actionItemId);
  if (typeof actionItem === 'undefined') {
    return {
      actionItem: {},
      isNewItem:true
    };
  }
  else {
    return {
      actionItem: actionItem,
      isNewItem: false
    };
  }
}

function mapDispatchToProps(dispatch) {
  return {
    actions: bindActionCreators(actionItemActions, dispatch)
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(EditActionItemPage);
