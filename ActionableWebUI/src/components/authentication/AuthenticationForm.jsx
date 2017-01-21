import React from 'react';
import TextInput from '../../common/TextInput';

export default class AuthenticationForm extends React.Component {
  constructor (props) {
    super (props);
  }

  render () {
    const {credentials, onChange, onAuthenticate, authenticating, errors} = this.props;
    const {username, password1, password2} = credentials;
    return (
      <form>
        <h1>Login</h1>
        <TextInput
          name="username"
          label="User name"
          value={username}
          onChange={onChange}
          error={errors.username}/>

        <TextInput
          name="password1"
          label="Password"
          value={password1}
          type="password"
          onChange={onChange}
          error={errors.password1}/>

        <TextInput
          name="password2"
          label="Password confirm"
          value={password2}
          type="password"
          onChange={onChange}
          error={errors.password2}/>

        <input
          type="submit"
          disabled={authenticating}
          value={authenticating ? 'Authenticating...' : 'Authenticate'}
          className="btn btn-primary"
          onClick={onAuthenticate}/>
      </form>
    )
  }
}

AuthenticationForm.propTypes = {
  credentials: React.PropTypes.object.isRequired,
  onAuthenticate: React.PropTypes.func.isRequired,
  onChange: React.PropTypes.func.isRequired,
  authenticating: React.PropTypes.bool,
  errors: React.PropTypes.object.isRequired
};
