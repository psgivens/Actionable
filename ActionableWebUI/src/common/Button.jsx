import React, {PropTypes} from 'react';

export default class Button extends React.Component {
    render () {
        const {disabled, onClick, text, ...rest} = this.props;
        return (
          <button
            type="button"
            className="btn btn-primary"
            onClick={onClick}
            disabled={disabled}>
              {text}
          </button>);
    }
}

Button.propTypes = {
  disabled: PropTypes.bool,
  onClick: PropTypes.func.isRequired,
  text: PropTypes.string.isRequired
};
