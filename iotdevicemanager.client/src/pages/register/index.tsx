import { useForm } from "react-hook-form"
import { NavLink } from "react-router-dom"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import { useState } from "react"
import { Button, Text, Field, Input, Title1, List, ListItem } from "@fluentui/react-components"
import { Checkmark20Filled, Dismiss20Filled } from '@fluentui/react-icons';

import { IdentityError, RegisteredUser } from "../../types"

const Schema = z.object({
    email: z.string().min(1, { message: "Email cannot be empty" }).email({ message: "Email must be of valid form" }),
    userName: z.string().min(1, {  message: "Username cannot be empty" }),
    password: z.string().min(8, { message: "Password must be at least 8 characters long, have a combination of upper and lower case letters, at least one number, and a non-numeric character." }),
    confirmPassword: z.string()
}).refine(data => data.password === data.confirmPassword, {
    message: "Passwords must match",
    path: ["confirmPassword"]
})

export const Register = () => {
    const { register, handleSubmit, formState: { errors, isSubmitting }, reset} = useForm<z.infer<typeof Schema>>({
        resolver: zodResolver(Schema),
        defaultValues: {
            email: "",
            userName: "",
            password: "",
            confirmPassword: ""
        }
    })
    const [httpErrors, setHttpErrors] = useState<IdentityError[]>([]);
    const [httpSuccess, setHttpSuccess] = useState("");
    const [pwContainsUpperCase, setPwContainsUpperCase] = useState(false);
    const [pwContainsLowerCase, setPwContainsLowerCase] = useState(false);
    const [pwContainsNumber, setPwContainsNumber] = useState(false);
    const [pwContainsSpecialCharacter, setPwContainsSpecialCharacter] = useState(false);
    const [pwLength, setPwLength] = useState(false);

    async function onSubmit(values: z.infer<typeof Schema>) {
        var response = await fetch("/api/Auth/register", {
            method: "POST",
            body: JSON.stringify(values),
            headers: {
                "Content-Type": "application/json"
            }
        })

        if (!response.ok) {
            var error = await response.json()
            setHttpErrors(error.errors)

            setTimeout(() => setHttpErrors([]), 5000)
        }
        else {
            var user: RegisteredUser = await response.json()
            setHttpSuccess(`User created with email ${user.email} and username ${user.username}`)

            reset()
        }
    }

    const onPasswordChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (["0","1","2","3","4","5","6","7","8","9"].some(v => event.target.value.includes(v))) {
            setPwContainsNumber(true);
        }
        else setPwContainsNumber(false)
        if (["@","%","+","!","#","$","?","~","_","-",":"].some(v => event.target.value.includes(v))) {
            setPwContainsSpecialCharacter(true);
        }
        else setPwContainsSpecialCharacter(false)
        if (/[A-Z]/.test(event.target.value)) {
            setPwContainsUpperCase(true)
        }
        else setPwContainsUpperCase(false)
        if (/[a-z]/.test(event.target.value)) {
            setPwContainsLowerCase(true)
        }
        else setPwContainsLowerCase(false)
        if (event.target.value.length >= 8) {
            setPwLength(true)
        }
        else setPwLength(false)
    }

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <div style={{ marginBottom: '1em'}}>
                <Title1>Register</Title1>
            </div>
            <form onSubmit={handleSubmit(onSubmit)}>
                {httpErrors &&
                    httpErrors.map((error) => (
                        <Text style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>{error.description}</Text>
                    ))
                }

                {httpSuccess && 
                <p style={{ color: 'green', width: '17em', height: '2em', borderRadius: '0.2em' }}>{httpSuccess}</p>}

                <div style={{ display:'flex', flexDirection: 'column', marginTop: '2em', width: '20vw' }}>
                    <Field label={"Email"} validationMessage={errors.email?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("email")}
                            type="email"
                            id="email"
                            placeholder="Email">
                        </Input>
                    </Field>

                    <Field label={"Username"} validationMessage={errors.userName?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("userName")}
                            id="userName"
                            placeholder="Username">
                        </Input>
                    </Field>

                    <Text weight="semibold">Password requirements</Text>
                    <List>
                        <div style={{ display: 'flex', flexDirection: 'row' }}>
                            <ListItem>
                                <Text style={{ fontSize: '0.9em' }}>8 characters long</Text>
                            </ListItem>
                            {pwLength
                                ? <Checkmark20Filled style={{ color: 'green' }} />
                                : <Dismiss20Filled style={{ color: 'red' }} /> }
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'row' }}>
                            <ListItem>
                                <Text style={{ fontSize: '0.9em' }}>At least one upper case letter</Text>
                            </ListItem>
                            {pwContainsUpperCase
                                ? <Checkmark20Filled style={{ color: 'green' }} />
                                : <Dismiss20Filled style={{ color: 'red' }} /> }
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'row' }}>
                            <ListItem>
                                <Text style={{ fontSize: '0.9em' }}>At least one lower case letter</Text>
                            </ListItem>
                            {pwContainsLowerCase
                                ? <Checkmark20Filled style={{ color: 'green' }}  />
                                : <Dismiss20Filled style={{ color: 'red' }} /> }
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'row' }}>
                            <ListItem>
                                <Text style={{ fontSize: '0.9em' }}>At least one number</Text>
                            </ListItem>
                            {pwContainsNumber
                                ? <Checkmark20Filled style={{ color: 'green' }}  />
                                : <Dismiss20Filled style={{ color: 'red' }} /> }
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'row' }}>
                            <ListItem>
                                <Text style={{ fontSize: '0.9em' }}>At least one special character (@, %, :, +, !, #, $, ?, ~, _, -)</Text>
                            </ListItem>
                            {pwContainsSpecialCharacter
                                ? <Checkmark20Filled style={{ color: 'green' }}  />
                                : <Dismiss20Filled style={{ color: 'red' }} /> }
                        </div>
                    </List>

                    <Field label={"Password"} validationMessage={errors.password?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("password")}
                            type="password"
                            id="password"
                            placeholder="Password"
                            onChange={onPasswordChange}>
                        </Input>
                    </Field>

                    <Field label={"Confirm password"} validationMessage={errors.confirmPassword?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("confirmPassword")}
                            type="password"
                            id="confirmPassword"
                            placeholder="Confirm your password">
                        </Input>
                    </Field>

                    <Button type="submit" 
                        disabled={isSubmitting} 
                        style={{ marginTop: '1em', backgroundColor: 'green', height: '2.5em' }}>
                        Register
                    </Button>
                </div>

                <div>
                    <div style={{ display:'flex', flexDirection: 'column', marginTop: '1em' }}>
                        <Text>Already have an account?</Text>
                        <NavLink to="/login">Sign in here.</NavLink> 
                    </div>
                </div>
            </form>
        </div>
    )
}