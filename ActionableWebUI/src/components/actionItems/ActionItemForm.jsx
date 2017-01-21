import React from 'react';
import TextInput from '../../common/TextInput';

export default class ActionItemForm extends React.Component {
  constructor (props, context) {
    super (props, context);
    const {actionItem} = props;
    this.state = {
      actionItem: actionItem
    };
  }

  render () {
//    const {actionItem} = this.state;
    const {actionItem, onChange, onSave, saving, errors} = this.props;
    const {fields, status} = actionItem;
    return (
      <form>
        <h1>Edit Action Item</h1>
        <TextInput
          name="actionable.title"
          label="Title"
          value={fields["actionable.title"]}
          onChange={onChange}
          error={errors.title}/>

        <TextInput
          name="actionable.description"
          label="Description"
          value={fields["actionable.description"]}
          onChange={onChange}
          error={errors.description}/>

        <TextInput
          name="actionable.status"
          label="Status"
          value={fields["actionable.status"]}
          onChange={onChange}
          error={errors.status}/>

        <input
          type="submit"
          disabled={saving}
          value={saving ? 'Saving...' : 'Save'}
          className="btn btn-primary"
          onClick={onSave}/>
      </form>
    );
  }
}

ActionItemForm.propTypes = {
  actionItem: React.PropTypes.object.isRequired,
  onSave: React.PropTypes.func.isRequired,
  onChange: React.PropTypes.func.isRequired,
  saving: React.PropTypes.bool,
  errors: React.PropTypes.object
};
