import { Component, PropTypes } from 'react'
import { connect } from 'react-redux'
import { withRouter } from 'react-router'
import {requestAuthLogout} from '../../actions/authenticationActions';
//import * as authActionCreators from '../actions/auth'

class LogoutPage extends Component {

  componentWillMount() {
    this.props.dispatch(requestAuthLogout())
    this.props.router.replace('/')
  }

  render() {
    return null
  }
}
LogoutPage.propTypes = {
  dispatch: PropTypes.func.isRequired,
  router: PropTypes.object.isRequired
}

export default connect()(withRouter(LogoutPage))
